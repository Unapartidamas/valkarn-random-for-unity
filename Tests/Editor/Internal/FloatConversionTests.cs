// Copyright (c) Una Partida Mas. All rights reserved.
// Licensed under the MIT license.

using NUnit.Framework;
using UnaPartidaMas.Valkarn.Random.Internal;

namespace UnaPartidaMas.Valkarn.Random.Tests
{
    [TestFixture]
    public class FloatConversionTests
    {
        [Test]
        public void ToFloat01_Zero_ReturnsZero()
        {
            Assert.AreEqual(0f, FloatConversion.ToFloat01(0u));
        }

        [Test]
        public void ToFloat01_MaxValue_LessThanOne()
        {
            float result = FloatConversion.ToFloat01(uint.MaxValue);
            Assert.Less(result, 1f);
            Assert.Greater(result, 0.999f);
        }

        [Test]
        public void ToFloat01_NeverReturnsOne()
        {
            // Test all values where top 24 bits are maxed
            for (uint i = 0; i < 256; i++)
            {
                float result = FloatConversion.ToFloat01(0xFFFFFF00u | i);
                Assert.Less(result, 1f, $"Returned >= 1.0 for input 0x{(0xFFFFFF00u | i):X8}");
            }
        }

        [Test]
        public void ToFloatNonZero_Zero_ReturnsPositive()
        {
            float result = FloatConversion.ToFloatNonZero(0u);
            Assert.Greater(result, 0f, "Must never return 0.0");
        }

        [Test]
        public void ToFloatNonZero_MaxValue_ReturnsOne()
        {
            float result = FloatConversion.ToFloatNonZero(uint.MaxValue);
            Assert.AreEqual(1f, result, 0.0001f, "Should reach 1.0 for max input");
        }

        [Test]
        public void ToFloatNonZero_NeverReturnsZero()
        {
            for (uint i = 0; i < 256; i++)
            {
                float result = FloatConversion.ToFloatNonZero(i);
                Assert.Greater(result, 0f, $"Returned 0.0 for input {i}");
            }
        }

        [Test]
        public void ToFloatSigned_ProducesNegativeValues()
        {
            // High bit set → negative after (int) cast and arithmetic right shift
            float result = FloatConversion.ToFloatSigned(0x80000000u);
            Assert.Less(result, 0f, "Should produce negative for bit 31 set");
        }

        [Test]
        public void ToFloatSigned_ProducesPositiveValues()
        {
            float result = FloatConversion.ToFloatSigned(0x7FFFFFFFu);
            Assert.Greater(result, 0f, "Should produce positive for bit 31 clear");
        }

        [Test]
        public void ToFloatSigned_Range_NegOneToOne()
        {
            // Test boundary values
            float min = FloatConversion.ToFloatSigned(0x80000000u); // most negative
            float max = FloatConversion.ToFloatSigned(0x7FFFFFFFu); // most positive

            Assert.GreaterOrEqual(min, -1f, $"Min value {min} < -1");
            Assert.Less(max, 1f, $"Max value {max} >= 1");

            // Verify the range actually spans close to [-1, 1), not a narrower interval
            Assert.Less(min, -0.9f, $"Min {min} should approach -1.0 (not just be negative)");
            Assert.Greater(max, 0.9f, $"Max {max} should approach 1.0 (not just be positive)");
        }

        [Test]
        public void ToFloatSigned_Zero_ReturnsZero()
        {
            Assert.AreEqual(0f, FloatConversion.ToFloatSigned(0u));
        }

        [Test]
        public void ToDouble01_Zero_ReturnsZero()
        {
            Assert.AreEqual(0.0, FloatConversion.ToDouble01(0UL));
        }

        [Test]
        public void ToDouble01_MaxValue_LessThanOne()
        {
            double result = FloatConversion.ToDouble01(ulong.MaxValue);
            Assert.Less(result, 1.0);
        }
    }
}
