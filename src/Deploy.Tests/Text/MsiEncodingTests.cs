using System;
using Xunit;

namespace Deploy.Text
{
    public class MsiEncodingTests
    {
        [Fact]
        public void EncodesSimple()
        {
            AssertEncoding("simple", false);
            AssertEncoding("simple", true);
        }

        [Fact]
        public void EncodesWithCharacters()
        {
            AssertEncoding("Validation_", false);
            AssertEncoding("Validation_", true);
            AssertEncoding("Columns_", false);
            AssertEncoding("Columns_", true);
        }

        [Fact]
        public void EncodesLong()
        {
            AssertEncoding("the quick brown fox jumped over the lazy dog", false);
            AssertEncoding("the quick brown fox jumped over the lazy dog", true);
        }

        [Fact]
        public void CanDecodeNonUnicode()
        {
            var expected = new Byte[]
            {
                0x52, 0x00, 0x6f, 0x00, 0x6f, 0x00, 0x74, 0x00,
                0x20, 0x00, 0x45, 0x00, 0x6e, 0x00, 0x74, 0x00,
                0x72, 0x00, 0x79, 0x00
            };

            var encoding = new MsiEncoding();

            Assert.Equal("Root Entry", encoding.GetString(expected));
        }

        [Fact]
        public void ValidationBytesAreEqual()
        {
            var expected = new byte[]
            {
                0x40, 0x48, 0xff, 0x3f, 0xe4, 0x43, 0xec, 0x41,
                0xe4, 0x45, 0xac, 0x44, 0x31, 0x48
            };

            var encoding = new MsiEncoding {IncludePreamble = true};

            Assert.Equal(expected, encoding.GetBytes("_Validation"));
            Assert.Equal("_Validation", encoding.GetString(expected));
        }

        [Fact]
        public void CanDecodeUnicode()
        {
            var expected = new byte[]
            {
                0x05, 0x00, 0x53, 0x00, 0x75, 0x00, 0x6d, 0x00,
                0x6d, 0x00, 0x61, 0x00, 0x72, 0x00, 0x79, 0x00,
                0x49, 0x00, 0x6e, 0x00, 0x66, 0x00, 0x6f, 0x00,
                0x72, 0x00, 0x6d, 0x00, 0x61, 0x00, 0x74, 0x00,
                0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00
            };

            var encoding = new MsiEncoding();

            Assert.Equal("\u0005SummaryInformation", encoding.GetString(expected));
        }

        private void AssertEncoding(string value, bool preamble)
        {
            var encoding = new MsiEncoding {IncludePreamble = preamble};

            var bytes = encoding.GetBytes(value);
            var encoded = encoding.GetString(bytes);

            Assert.Equal(value, encoded);
        }
    }
}
