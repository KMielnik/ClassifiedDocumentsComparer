using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace ClassifiedDocumentsComparer
{
    public class MAPCalculator
    {
        public double GetMAP()
        {
            Directory.CreateDirectory("user");
            Directory.CreateDirectory("generated");

            var data = Directory.EnumerateFiles("user")
                .Select(x => x.Substring(x.LastIndexOf("\\") + 1))
                .AsParallel()
                .Select(x => CalculateOneImage(x))
                .ToList();

            var sum = data
                .Aggregate(new ClassificationData(), (acc, x) =>
                     {
                         acc.Text.fn += x.Text.fn;
                         acc.Text.fp += x.Text.fp;
                         acc.Text.tp += x.Text.tp;

                         acc.Sign.fn += x.Sign.fn;
                         acc.Sign.fp += x.Sign.fp;
                         acc.Sign.tp += x.Sign.tp;

                         acc.Stamp.fn += x.Stamp.fn;
                         acc.Stamp.fp += x.Stamp.fp;
                         acc.Stamp.tp += x.Stamp.tp;

                         return acc;
                     });

            var precisionRecall = data
                .Select((x, i) =>
                {
                    var textPrecision = (double)(data.Take(i + 1).Sum(y => y.Text.tp)) / (double)(i + 1);
                    var textRecall = (double)(data.Take(i + 1).Sum(y => y.Text.tp)) / (double)(sum.Text.tp + sum.Text.fn);

                    var stampPrecision = (double)(data.Take(i + 1).Sum(y => y.Stamp.tp)) / (double)(i + 1);
                    var stampRecall = (double)(data.Take(i + 1).Sum(y => y.Stamp.tp)) / (double)(sum.Stamp.tp + sum.Stamp.fn);

                    var signPrecision = (double)(data.Take(i + 1).Sum(y => y.Sign.tp)) / (double)(i + 1);
                    var signRecall = (double)(data.Take(i + 1).Sum(y => y.Sign.tp)) / (double)(sum.Sign.tp + sum.Sign.fn);

                    return (text:(textPrecision, textRecall), stamp:(stampPrecision, stampRecall), sign:(signPrecision, signRecall));
                })
                .ToList();

            var alignedPR = precisionRecall
                .Select((x, i) =>
                {
                    x.text.textPrecision = precisionRecall.Skip(i).Max(x => x.text.textPrecision);
                    x.stamp.stampPrecision = precisionRecall.Skip(i).Max(x => x.stamp.stampPrecision);
                    x.sign.signPrecision = precisionRecall.Skip(i).Max(x => x.sign.signPrecision);

                    return x;
                })
                .ToList();

            double textIntegral = alignedPR[0].text.textRecall * alignedPR[0].text.textPrecision;
            double stampIntegral = alignedPR[0].stamp.stampRecall * alignedPR[0].stamp.stampPrecision;
            double signIntegral = alignedPR[0].sign.signRecall * alignedPR[0].sign.signPrecision;
            
            for (int i=0;i<alignedPR.Count-1;i++)
            {
                textIntegral += (alignedPR[i + 1].text.textRecall - alignedPR[i].text.textRecall) * alignedPR[i].text.textPrecision;
                stampIntegral += (alignedPR[i + 1].stamp.stampRecall - alignedPR[i].stamp.stampRecall) * alignedPR[i].stamp.stampPrecision;
                signIntegral += (alignedPR[i + 1].sign.signRecall - alignedPR[i].sign.signRecall) * alignedPR[i].sign.signPrecision;
            }

            Console.WriteLine($"Text AP: {textIntegral}");
            Console.WriteLine($"Stamp AP: {stampIntegral}");
            Console.WriteLine($"Sign AP: {signIntegral}");

            return (textIntegral + stampIntegral + signIntegral) / 3;
        }

        private ClassificationData CalculateOneImage(string name)
        {
            var userBMP = (Bitmap)Image.FromFile($"user\\{name}");
            var generatedBMP = (Bitmap)Image.FromFile($"generated\\mask_{name}");

            return GetData(userBMP, generatedBMP);
        }

        private ClassificationData GetData(Bitmap userBMP, Bitmap generatedBMP)
        {
            const double iouThreshold = 0.5;

            var data = new ClassificationData();

            var userText = GetThresholdedImage(userBMP, DocumentClasses.Text);
            var generatedText = GetThresholdedImage(generatedBMP, DocumentClasses.Text);

            var userStamp = GetThresholdedImage(userBMP, DocumentClasses.Stamp);
            var generatedStamp = GetThresholdedImage(generatedBMP, DocumentClasses.Stamp);

            var userSign = GetThresholdedImage(userBMP, DocumentClasses.Sign);
            var generatedSign = GetThresholdedImage(generatedBMP, DocumentClasses.Sign);

            var iouText = GetIoU(
                userText,
                generatedText
                );

            var iouTextStamp = GetIoU(
                userText,
                generatedStamp
                );

            var iouTextSign = GetIoU(
                userText,
                generatedSign
                );

            if (iouTextStamp > iouThreshold || iouTextSign > iouThreshold)
                data.Text = (0, 0, 1);
            else if (iouText >= iouThreshold)
                data.Text = (1, 0, 0);
            else if (iouText < iouThreshold)
                data.Text = (0, 1, 0);


            var iouStamp = GetIoU(
                userStamp,
                generatedStamp
                );

            var iouStampText = GetIoU(
                userStamp,
                generatedText
                );

            var iouStampSign = GetIoU(
                userStamp,
                generatedSign
                );

            if (iouStampText > iouThreshold || iouStampSign > iouThreshold)
                data.Stamp = (0, 0, 1);
            else if (iouStamp >= iouThreshold)
                data.Stamp = (1, 0, 0);
            else if (iouStamp < iouThreshold)
                data.Stamp = (0, 1, 0);


            var iouSign = GetIoU(
                userSign,
                generatedSign
                );

            var iouSignText = GetIoU(
                userSign,
                generatedText
                );

            var iouSignStamp = GetIoU(
                userSign,
                generatedStamp
                );

            if (iouSignText > iouThreshold || iouSignStamp > iouThreshold)
                data.Sign = (0, 0, 1);
            else if (iouSign >= iouThreshold)
                data.Sign = (1, 0, 0);
            else if (iouSign < iouThreshold)
                data.Sign = (0, 1, 0);

            return data;
        }

        private double GetIoU(Bitmap mask1,Bitmap mask2)
        {
            double overlap = 0;
            double union = 0;

            for (int i = 0; i < mask1.Width; i++)
                for (int j = 0; j < mask1.Height; j++)
                {
                    if (mask1.GetPixel(i, j).CompareRGB(Color.White) && mask2.GetPixel(i, j).CompareRGB(Color.White))
                        overlap++;
                    if (mask1.GetPixel(i, j).CompareRGB(Color.White) || mask2.GetPixel(i, j).CompareRGB(Color.White))
                        union++;
                }

            return overlap / union;
        }

        private Bitmap GetThresholdedImage(Bitmap img, (string Name, Color Color) documentClass)
        {
            var result = new Bitmap(img.Width, img.Height);

            for (int i = 0; i < img.Width; i++)
                for (int j = 0; j < img.Height; j++)
                    if (img.GetPixel(i, j).CompareRGB(documentClass.Color))
                        result.SetPixel(i, j, Color.White);
                    else
                        result.SetPixel(i, j, Color.Black);

            return result;
        }
    }
}
