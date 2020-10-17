using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Faces.ConsoleTest
{
    public class ImageUtility
    {
        public byte[] ConvertToBytes(string imagePath)
        {
            var memoryStream = new MemoryStream();

            using (var stream = new FileStream(imagePath, FileMode.Open))
            {
                stream.CopyTo(memoryStream);
            }

            var bytes = memoryStream.ToArray();

            return bytes;
        }

        public void FromBytesToImage(byte[] imageBytes, string fileName)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                var img = Image.FromStream(ms);

                img.Save(fileName + ".jpg", ImageFormat.Jpeg);
            }
        }
    }
}