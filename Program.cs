using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace ClassifiedDocumentsComparer
{
    class Program
    {
        static void Main(string[] args)
        {
            var calculator = new MAPCalculator();

            var mapData = calculator.GetMAP();

            File.Delete("results.exe");
            File.AppendAllText("results.txt", $"AP_TEXT;{mapData.textAP}\n");
            File.AppendAllText("results.txt", $"AP_STAMP;{mapData.stampAP}\n");
            File.AppendAllText("results.txt", $"AP_SIGN;{mapData.signAP}\n");
            File.AppendAllText("results.txt", $"MAP;{(mapData.signAP + mapData.stampAP + mapData.textAP) / 3}\n");


            File.AppendAllText("results.txt", $"\n");

            OldMethod();
        }

        static void OldMethod()
        {
            Directory.CreateDirectory("user");
            Directory.CreateDirectory("generated");

            var percentages = Directory.EnumerateFiles("user")
                .Select(x => x.Substring(x.LastIndexOf("\\") + 1))
                .Select(x => (name:x, percentage: Compare(x)))
                .ToList();

            percentages
                .Select(x => $"{x.name};{Math.Round(x.percentage, 2)}")
                .ToList()
                .ForEach(x => File.AppendAllText("results.txt", x+"\n"));
        }

        static double Compare(string name)
        {
            const double eps = 10;
            var userBMP = (Bitmap)Image.FromFile($"user\\{name}");
            var generatedBMP = (Bitmap)Image.FromFile($"generated\\mask_{name}");

            int missalignedPixels = 0;

            for (int i = 0; i < userBMP.Width; i++)
                for (int j = 0; j < userBMP.Height; j++)
                {
                    var userPixel = userBMP.GetPixel(i, j);
                    var generatedPixel = generatedBMP.GetPixel(i, j);

                    double diff = 0;

                    diff += Math.Abs(userPixel.R - generatedPixel.R);
                    diff += Math.Abs(userPixel.G - generatedPixel.G);
                    diff += Math.Abs(userPixel.B - generatedPixel.B);

                    if (diff > eps)
                        missalignedPixels++;
                }

            double allPixels = userBMP.Width * userBMP.Height;

            return (allPixels - missalignedPixels) / allPixels * 100;
        }
    }
}
