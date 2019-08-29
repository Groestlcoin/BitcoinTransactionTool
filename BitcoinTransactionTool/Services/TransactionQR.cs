using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using QRCoder;

namespace BitcoinTransactionTool.Services {
    public class TransactionQR {
        public static BitmapImage Build(string tx) {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(tx, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            Bitmap qrCodeImage = qrCode.GetGraphic(20,Color.FromArgb(255,39,232,167) ,Color.FromArgb(255,80,100,119), null, 15);

            using (MemoryStream memory = new MemoryStream()) {
                qrCodeImage.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
