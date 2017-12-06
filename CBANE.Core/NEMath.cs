using System;
using System.Linq;

namespace CBANE.Core
{
    public static class NEMath
    {
        public static Random RNG = new Random();

        /// <summary>
        /// <para>Returns a random double between 0 and 1.</para>
        /// <para>A biasing power greater than 0 but less than 1 (0.2, 0.3, etc) will bias the result towards 1.</para>
        /// <para>A biasing power greater 1 (2, 3, etc) will bias the result towards 0.</para>
        /// </summary>
        public static double Random(double biasingPower = 1)
        {
            return Math.Pow(NEMath.RNG.NextDouble(), biasingPower);
        }

        public static double RandomBetween(double min, double max, double biasingPower = 1)
        {
            return NEMath.Random(biasingPower) * (max - min) + min;
        }

        public static double Softstep(double x)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        public static double Softplus(double x)
        {
            var y = 1 + Math.Exp(x);

            if (y <= 0)
                return 0;

            return Math.Log(y);
        }

        public static double ReLU(double x)
        {
            return (x < 0) ? 0 : x;
        }

        public static double LeakyReLU(double x)
        {
            return (x < 0) ? (0.01 * x) : x;
        }

        public static double Softmax(double[] vector, int targetIndex)
        {
            if (vector == null || vector.Length <= targetIndex || targetIndex < 0)
                return 0;

            var vectorExps = vector.Select(n => Math.Round(Math.Exp(n), 6)).ToArray();
            var sumExps = Math.Round(vectorExps.Sum(), 6);
            var softmax = vectorExps.Select(n => Math.Round((n / sumExps), 6)).ToArray();

            return softmax[targetIndex];
        }

        public static double Clamp(double x, double min, double max)
        {
            var y = x;

            y = (y < min) ? min : y;
            y = (y > max) ? max : y;

            return y;
        }

        /// <summary>
        /// Returns the dot product of two vectors with the same dimensions.
        /// </summary>
        public static double DotVectors(double[] vector1, double[] vector2)
        {
            // If either vector is null, or the two vectors are not of the
            // same dimensions, we can't by definition return a dot product.
            if(vector1 == null || vector2 == null || vector1?.Length != vector2?.Length)
                return double.NaN;

            if(vector1.Length == 0 || vector2.Length == 0)
                return 0;

            double product = 0;

            for (var i = 0; i < vector1.Length; i++)
                product += vector1[i] * vector2[i];

            return product;
        }

        /// <summary>
        /// Returns the length of a vector (not the dimensions).
        /// </summary>
        public static double VectorLength(double[] vector)
        {
            if(vector == null)
                return double.NaN;

            if(vector.Length == 0)
                return 0;

            return Math.Sqrt(vector.Select(n => n * n).Sum());;
        }

        /// <summary>
        /// Returns the angle between two vectors with the same dimensions.
        /// </summary>
        /// <returns>Angle between vectors in degrees.</returns>
        public static double AngleBetweenVectors(double[] vector1, double[] vector2)
        {
            var dot = DotVectors(vector1, vector2);

            if(double.IsNaN(dot))
                return double.NaN;

            if(dot == 0)
                return 0;

            var l1 = VectorLength(vector1);
            var l2 = VectorLength(vector2);

            var theta = dot / (l1 * l2);

            return Math.Acos(theta) * (180 / Math.PI);
        }

    }
}
