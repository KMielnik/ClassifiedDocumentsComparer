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
            Directory.CreateDirectory("user");
            Directory.CreateDirectory("generated");

            Directory.EnumerateFiles("user")
                .Select(x => x.Substring(x.LastIndexOf("\\") + 1))
                .Select(x => $"Dla dokumentu: {x}\t Procent zgodnosci: {Math.Round(Compare(x), 2)}%")
                .ToList()
                .ForEach(Console.WriteLine);
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
