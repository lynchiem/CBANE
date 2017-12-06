using System;
using Xunit;

using CBANE.Core;
using static CBANE.Core.NEMath;

namespace CBANE.Tests
{
    public class NEMathShould
    {
        [Fact]
        public void ReturnNaNLengthForNullVector()
        {
            double[] vector = null;

            double length = VectorLength(vector);

            Assert.True(double.IsNaN(length), "Vector length of NULL should be NaN");
        }

        [Fact]
        public void ReturnZeroLengthForZeroDimensionVector()
        {
            double[] vector = new double[0];

            double length = VectorLength(vector);

            Assert.True(length == 0, "Vector length of [] should be 0");
        }

        [Fact]
        public void ReturnCorrectLengthForValidVector()
        {
            double[] vector = null;
            double length = double.MinValue;

            // Vector Length of [1, 2] = 2.23607
            vector = new double[] {1, 2};
            length = Math.Round(VectorLength(vector), 5);

            Assert.True(length == 2.23607, "Vector length of [1, 2] should be 2.23607");

            // Vector Length of [0.15, 0.98, 0.743] = 1.24
            vector = new double[] {0.15, 0.98, 0.743};
            length = Math.Round(VectorLength(vector), 2);

            Assert.True(length == 1.24, "Vector length of [0.15, 0.98, 0.743] should be 1.24");
        }
        
        public void ReturnComponentAsLengthForOneDimensionVector()
        {
            double[] vector = null;
            double length = double.MinValue;

            // Vector Length of [1] = 1
            vector = new double[] {1};
            length = VectorLength(vector);

            Assert.True(length == 1, "Vector length of [1] should be 1");

            // Vector Length of [-900] = -900
            vector = new double[] {-900};
            length = VectorLength(vector);

            Assert.True(length == -900, "Vector length of [-900] should be -900");

            // Vector Length of [0.234] = 0.234
            vector = new double[] {0.234};
            length = VectorLength(vector);

            Assert.True(length == 0.234, "Vector length of [0.234] should be 0.234");
        }

        [Fact]
        public void ReturnCorrectDotProductForValidVectors()
        {
            double[] v1, v2 = null;
            double product = double.MinValue;

            // [1] dot [-9] = -9
            v1 = new double[] {1};
            v2 = new double[] {-9};
            
            product = DotVectors(v1, v2);
            
            Assert.True(product == -9, "[1] dot [-9] should be -9");

            // [-25, 50] dot [6, -12] = -750
            v1 = new double[] {-25, 50};
            v2 = new double[] {6, -12};
            
            product = DotVectors(v1, v2);
            
            Assert.True(product == -750, "[-25, 50] dot [6, -12] should be -750");

            // [0, 0.5, -0.95, 0.25] dot [1, -1, 0.75, 0.01] = -1.21
            v1 = new double[] {0, 0.5, -0.95, 0.25};
            v2 = new double[] {1, -1, 0.75, 0.01};
            
            product = DotVectors(v1, v2);
            
            Assert.True(product == -1.21, "[0, 0.5, -0.95, 0.25] dot [1, -1, 0.75, 0.01] should be -1.21");
        }
        
        [Fact]
        public void ReturnZeroAngleBetweenOneDimensionVectorsOfSameSign()
        {
            double[] v1, v2 = null;
            double angle = double.MinValue;

            // Angle Between [1] & [-9] != 0
            v1 = new double[] {1};
            v2 = new double[] {-9};
            
            angle = AngleBetweenVectors(v1, v2);

            Assert.False(angle == 0, "Angle between [1] and [-9] should not be 0");

            // Angle Between [-1] & [-999] = 0
            v1 = new double[] {-1};
            v2 = new double[] {-999};
            
            angle = AngleBetweenVectors(v1, v2);

            Assert.True(angle == 0, "Angle between [-1] and [-999] should be 0");

            // Angle Between [0] & [0] = 0
            v1 = new double[] {0};
            v2 = new double[] {0};
            
            angle = AngleBetweenVectors(v1, v2);

            Assert.True(angle == 0, "Angle between [0] and [0] should be 0");
        }

        [Fact]
        public void ReturnMaxAngleBetweenOneDimensionVectorsOfOppositeSign()
        {
            double[] v1, v2 = null;
            double angle = double.MinValue;

            // Angle Between [1] & [-9] = 180
            v1 = new double[] {1};
            v2 = new double[] {-9};
            
            angle = AngleBetweenVectors(v1, v2);

            Assert.True(angle == 180, "Angle between [1] and [-9] should be 180");

            // Angle Between [-5] & [50] = 180
            v1 = new double[] {-5};
            v2 = new double[] {50};
            
            angle = AngleBetweenVectors(v1, v2);

            Assert.True(angle == 180, "Angle between [-5] and [50] should be 180");
        }

        [Fact]
        public void ReturnZeroAnglebetweenZeroDimensionVectors()
        {
            double[] v1 = new double[0];
            double[] v2 = new double[0];

            double angle = AngleBetweenVectors(v1, v2);

            Assert.True(angle == 0, "Angle between [] and [] should be 0");
        }

        [Fact]
        public void ReturnNaNAngleBetweenNullVectors()
        {
            double[] v1, v2 = null;
            double angle = double.MinValue;

            // Angle Between [1, 2, 3] & NULL = NaN
            v1 = new double[] {1, 2, 3};
            v2 = null;
            
            angle = AngleBetweenVectors(v1, v2);

            Assert.True(double.IsNaN(angle), "Angle between [1] and NULL should be NaN");

            // Angle Between NULL & [1, 2] = NaN
            v1 = null;
            v2 = new double[] {1, 2};
            
            angle = AngleBetweenVectors(v1, v2);

            Assert.True(double.IsNaN(angle), "Angle between NULL and [1, 2] should be NaN");

            // Angle Between NULL & NULL = NaN
            v1 = null;
            v2 = null;
            
            angle = AngleBetweenVectors(v1, v2);
            
            Assert.True(double.IsNaN(angle), "Angle between NULL and NULL should be NaN");
        }

        [Fact]
        public void ReturnNaNAngleBetweenMixedDimensionVectors()
        {
            double[] v1, v2 = null;
            double angle = double.MinValue;

            // Angle Between [1, 2] & [1] = NaN
            v1 = new double[] {1, 2};
            v2 = new double[] {1};
            
            angle = AngleBetweenVectors(v1, v2);

            Assert.True(double.IsNaN(angle), "Angle between mixed dimension vectors [1, 2] and [1] should be NaN.");

            // Angle Between [9] & [1, 2, 3, 4, 5] = NaN
            v1 = new double[] {9};
            v2 = new double[] {1, 2, 3, 4, 5};
            
            angle = AngleBetweenVectors(v1, v2);

            Assert.True(double.IsNaN(angle), "Angle between mixed dimension vectors [9] and [1, 2, 3, 4, 5] should be NaN.");
        }

        [Fact]
        public void ReturnCorrectAngleBetweenValidVectors()
        {
            double[] v1, v2 = null;
            double angle = double.MinValue;

            // Angle Between [2, 9, -3] & [-3, -4, 8] = 136.2
            v1 = new double[] {2, 9, -3};
            v2 = new double[] {-3, -4, 8};
            
            angle = Math.Round(AngleBetweenVectors(v1, v2), 1);

            Assert.True(angle == 136.2, "Angle between [2, 9, -3] and [-3, -4, 8] should be 136.2");

            // Angle Between [12, -4.5] & [-3, 21] = 118.69
            v1 = new double[] {12, -4.5};
            v2 = new double[] {-3, 21};
            
            angle = Math.Round(AngleBetweenVectors(v1, v2), 2);
            
            Assert.True(angle == 118.69, "Angle between [12, -4.5] and [-3, 21] should be 118.69");

            // Angle Between [0.113, 0.123, 1, 0, 0.345] & [0, 1, 1, 0.3432, 0.97] = 38.89945
            v1 = new double[] {0.113, 0.123, 1, 0, 0.345};
            v2 = new double[] {0, 1, 1, 0.3432, 0.97};
            
            angle = Math.Round(AngleBetweenVectors(v1, v2), 5);
            
            Assert.True(angle == 38.89945, "Angle between [0.113, 0.123, 1, 0, 0.345] and [0, 1, 1, 0.3432, 0.97] should be 38.89945");
        }

    }
}
