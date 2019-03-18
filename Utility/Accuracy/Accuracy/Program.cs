using System;
using System.IO;

namespace Core.LanguageIdentifier
{
    class Accuracy
    {
        static void Main(string[] args)
        {
            var lines = File.ReadAllLines(args[0]);
            var langs = lines[0].Split('\t');
            var n = lines.Length - 1;
            var matrix = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                var line = lines[i + 1].Split('\t');
                for (int j = 0; j < n; j++)
                {
                    matrix[i, j] = Double.Parse(line[j + 1]);
                }
            }
            var diagSum = 0.0;
            var matrixSum = 0.0;
            var weightSum = 0.0;
            var weightedAccuracy = 0.0;
            for (int i = 0; i < n; i++)
            {
                double rowSum = 0;
                for (int j = 0; j < n; j++)
                {
                    rowSum += matrix[j, i];
                }
                diagSum += matrix[i, i];
                matrixSum += rowSum;
                var accuracy = matrix[i, i] / rowSum;
                Console.WriteLine("  {0} {1,6:f4}", langs[i + 1], accuracy);
                if (accuracy == 1.0) accuracy = 0.999999;
                if (accuracy == 0.0) accuracy = 0.000001;
                var weight = 1.0 / (Math.Sqrt(accuracy * (1.0 - accuracy) / rowSum));
                weightSum += weight;
                weightedAccuracy += weight * accuracy;
            }
            Console.WriteLine("Unweighted accuracy = {0,6:f4}", diagSum / matrixSum);
            Console.WriteLine("  Weighted accuracy = {0,6:f4}", weightedAccuracy / weightSum);
        }
    }
}