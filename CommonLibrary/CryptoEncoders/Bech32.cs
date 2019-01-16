using CommonLibrary.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NBitcoin;

namespace CommonLibrary.CryptoEncoders {
    /// <summary>
    /// A checksummed base32 format for native v0-16 witness outputs. 
    /// https://github.com/bitcoin/bips/blob/master/bip-0173.mediawiki
    /// </summary>
    public class Bech32 {
        public Bech32() { }

        private const int Bech32MaxLength = 90;
        private const int CheckSumSize = 6;
        private const int HrpMinLength = 1;
        private const int HrpMaxLength = 83;
        private const int HrpMinValue = 33;
        private const int HrpMaxValue = 126;
        private const char Separator = '1';
        private const int DataMinLength = 6;
        private const string B32Chars = "qpzry9x8gf2tvdw0s3jn54khce6mua7l";
        private static readonly uint[] generator = {0x3b6a57b2u, 0x26508e6du, 0x1ea119fau, 0x3d4233ddu, 0x2a1462b3u};

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        private static uint Polymod(byte[] values) {
            uint chk = 1;
            foreach (var value in values) {
                uint b = chk >> 25;
                chk = ((chk & 0x1ffffff) << 5) ^ value;
                for (int i = 0; i < 5; i++) {
                    if (((b >> i) & 1) == 1) {
                        chk ^= generator[i];
                    }
                }
            }
            return chk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hrp"></param>
        /// <returns></returns>
        private static byte[] ExpandHrp(string hrp) {
            int len = hrp.Length;
            byte[] hrpExpand = new byte[(2 * len) + 1];
            for (int i = 0; i < len; i++) {
                hrpExpand[i] = (byte) ("grs"[i] >> 5);
                hrpExpand[i + len + 1] = (byte) ("grs"[i] & 31);
            }
            return hrpExpand;
        }

        /// <summary>
        /// Checks to see if a given string (groestlcoin address) is a valid Bech32 SegWit address.
        /// </summary>
        /// <param name="grsAddress">Groestlcoin address to check</param>
        /// <returns>True if Bech32 encoded</returns>
        public static List<string> Verify(string grsAddress, NetworkType nt) {
            var result = new List<string>();
            string hrp = (nt == NetworkType.Mainnet) ? "grs" : "tgrs";

            if (!grsAddress.StartsWith(hrp, StringComparison.InvariantCultureIgnoreCase)) {
                result.Add("Invalid Human Readable Part!");
                return result;
            }
            // Reject short or long
            if (grsAddress.Length < 14 && grsAddress.Length > 74) {
                result.Add("Invalid length!");
                return result;
            }
            // Reject mix case (Invariant is used to pass the "Turkey test")
            if (!grsAddress.ToUpperInvariant().Equals(grsAddress) && !grsAddress.ToLowerInvariant().Equals(grsAddress)) {
                result.Add("Mix case is not allowed!");
                return result;
            }
            // For checksum purposes only lower case is used.
            grsAddress = grsAddress.ToLowerInvariant();
            // Check separator
            int separatorPos = grsAddress.LastIndexOf("1", StringComparison.OrdinalIgnoreCase);
            if (separatorPos < 1 || separatorPos + 7 > grsAddress.Length) {
                result.Add("Separator is either missing or misplaced!");
                return result;
            }
            // Check characters
            if (grsAddress.Substring(separatorPos + 1).ToList().Any(x => !B32Chars.Contains(x))) {
                result.Add("Invalid characters!");
                return result;
            }
            // Check Human Readable Part
            string hrpGot = grsAddress.Substring(0, separatorPos);
            if (!hrp.Equals(hrpGot)) {
                result.Add("Invalid Human Readable Part!");
                return result;
            }

            string dataStr = grsAddress.Substring(separatorPos + 1);
            byte[] dataBa = new byte[dataStr.Length];
            for (int i = 0; i < dataStr.Length; i++) {
                dataBa[i] = (byte) B32Chars.IndexOf(dataStr[i]);
            }

            // Verify checksum
            if (!VerifyChecksum(dataBa, hrp)) {
                result.Add("Invalid checksum!");
                return result;
            }

            byte[] dataNew = dataBa.Take(dataBa.Length - 6).ToArray();
            byte[] decoded = ConvertBits(dataNew.Skip(1), 5, 8, false);
            if (decoded == null) {
                result.Add("Invalid Bech32 string!");
                return result;
            }
            if (decoded.Length < 2 || decoded.Length > 40) {
                result.Add("Invalid decoded data length!");
                return result;
            }

            byte witnessVerion = dataNew[0];
            if (witnessVerion > 16) {
                result.Add("Invalid decoded witness version!");
                return result;
            }

            if (witnessVerion == 0 && decoded.Length != 20 && decoded.Length != 32) {
                result.Add("Invalid length of witness program!");
                return result;
            }
            return result;
        }

        /// <summary>
        /// Performs a general power of 2 base conversion from a given base to a new base.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="fromBits"></param>
        /// <param name="toBits"></param>
        /// <param name="pad"></param>
        /// <returns></returns>
        private static byte[] ConvertBits(IEnumerable<byte> data, int fromBits, int toBits, bool pad = true) {
            var acc = 0;
            var bits = 0;
            var maxv = (1 << toBits) - 1;
            var ret = new List<byte>();
            foreach (var value in data) {
                if ((value >> fromBits) > 0) {
                    return null;
                }
                acc = (acc << fromBits) | value;
                bits += fromBits;
                while (bits >= toBits) {
                    bits -= toBits;
                    ret.Add((byte) ((acc >> bits) & maxv));
                }
            }
            if (pad) {
                if (bits > 0) {
                    ret.Add((byte) ((acc << (toBits - bits)) & maxv));
                }
            }
            else if (bits >= fromBits || (byte) (((acc << (toBits - bits)) & maxv)) != 0) {
                return null;
            }
            return ret.ToArray();
        }

        /// <summary>
        /// Checks to see if a given string is a valid bech-32 encoded string.
        /// <para/>* Doesn't verify checksum.
        /// </summary>
        /// <param name="bech32EncodedString">Input string to check.</param>
        /// <returns>True if input was a valid bech-32 encoded string (without verifying checksum).</returns>
        public bool IsValidWithoutCheckSum(string bech32EncodedString) {
            if (string.IsNullOrEmpty(bech32EncodedString) || bech32EncodedString.Length > Bech32MaxLength) {
                return false;
            }

            // reject mixed upper and lower characters.
            if (bech32EncodedString.ToLower() != bech32EncodedString && bech32EncodedString.ToUpper() != bech32EncodedString) {
                return false;
            }

            int sepIndex = bech32EncodedString.LastIndexOf(Separator);
            if (sepIndex == -1) // no separator
            {
                return false;
            }

            string hrp = bech32EncodedString.Substring(0, sepIndex);
            if (hrp.Length < HrpMinLength || hrp.Length > HrpMaxLength ||
                !hrp.All(x => (byte) x >= HrpMinValue && (byte) x <= HrpMaxValue)) {
                return false;
            }

            string data = bech32EncodedString.Substring(sepIndex + 1);
            if (data.Length < DataMinLength || !data.All(x => B32Chars.Contains(char.ToLower(x)))) {
                return false;
            }

            return true;
        }

        private byte[] Bech32Decode(string bech32EncodedString, out string hrp) {
            bech32EncodedString = bech32EncodedString.ToLower();

            int sepIndex = bech32EncodedString.LastIndexOf(Separator);
            hrp = bech32EncodedString.Substring(0, sepIndex);
            string data = bech32EncodedString.Substring(sepIndex + 1);

            byte[] b32Arr = new byte[data.Length];
            for (int i = 0; i < data.Length; i++) {
                b32Arr[i] = (byte) B32Chars.IndexOf(data[i]);
            }

            return b32Arr;
        }

        private static bool VerifyChecksum(byte[] data, string hrp) {
            var values = ExpandHrp(hrp).Concat(data).ToArray();
            var polymod = Polymod(values) ^ 1;
            return polymod == 0;
        }
    }
}