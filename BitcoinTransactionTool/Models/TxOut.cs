using System.Collections.Generic;
using NBitcoin.Altcoins;

namespace NBitcoin {
    public class TxOutLists : NBitcoin.TxOutList {
        public List<TxOuts> TxOuts { get; set; } = new List<TxOuts>();
    }

    public class TxOuts : NBitcoin.TxOut {
        private BitcoinAddress _pubKey;

        public BitcoinAddress PubKey => ScriptPubKey.GetDestinationAddress(Groestlcoin.Instance.Mainnet);
    }
}