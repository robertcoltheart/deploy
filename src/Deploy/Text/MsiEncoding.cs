using System;
using System.Text;

namespace Deploy.Text
{
    internal class MsiEncoding : Encoding
    {
        private const string Base64Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz._";

        public bool IncludePreamble { get; set; }

        public override int GetByteCount(char[] chars, int index, int count)
        {
            var byteCount = 0;
            var sourceIndex = index;

            if (IncludePreamble)
            {
                byteCount += 2;
            }

            while (sourceIndex < index + count)
            {
                var current = chars.GetOrDefault(sourceIndex++);
                var next = chars.GetOrDefault(sourceIndex);

                if (Base64Chars.IndexOf(current) != -1 && Base64Chars.IndexOf(next) != -1)
                {
                    sourceIndex++;
                }

                byteCount += 2;
            }

            return byteCount;
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            var sourceIndex = charIndex;
            var destIndex = byteIndex;

            if (IncludePreamble)
            {
                bytes[destIndex++] = 0x40;
                bytes[destIndex++] = 0x48;
            }

            while (sourceIndex < charIndex + charCount)
            {
                var current = chars.GetOrDefault(sourceIndex++);
                var next = chars.GetOrDefault(sourceIndex);

                var encoded = EncodeBase64(current);
                var nextEncoded = EncodeBase64(next);

                if (encoded != -1 && nextEncoded != -1)
                {
                    var c1 = encoded + 0x4800;
                    var c2 = (nextEncoded + 0x3ffffc0) << 6;

                    bytes.Write((ushort) (c1 + c2), destIndex);
                    sourceIndex++;
                }
                else if (encoded != -1)
                {
                    var c1 = encoded + 0x4800;

                    bytes.Write((ushort) c1, destIndex);
                }
                else
                {
                    bytes.Write(current, destIndex);
                }

                destIndex += 2;
            }

            return destIndex - byteIndex;
        }

        private int EncodeBase64(char c)
        {
            return Base64Chars.IndexOf(c);
        }

        private char DecodeBase64(int c)
        {
            return Base64Chars[c];
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            var charCount = 0;

            for (var i = index; i < count; i += 2)
            {
                var value = BitConverter.ToUInt16(bytes, i);

                if (value >= 0x3800 && value < 0x4800)
                {
                    charCount += 2;
                }
                else if (value != 0x4840)
                {
                    charCount++;
                }
            }

            return charCount;
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            var index = charIndex;

            for (var i = byteIndex; i < byteCount; i += 2)
            {
                var value = BitConverter.ToUInt16(bytes, i);

                if (value >= 0x3800 && value < 0x4800)
                {
                    var c = value - 0x3800;

                    chars[index++] = DecodeBase64(c & 0x3f);
                    chars[index++] = DecodeBase64((c >> 6) & 0x3f);
                }
                else if (value >= 0x4800 && value < 0x4840)
                {
                    chars[index++] = DecodeBase64(value - 0x4800);
                }
                else if (value != 0x4840)
                {
                    chars[index++] = (char) value;
                }
            }

            return index - charIndex;
        }

        public override int GetMaxByteCount(int charCount)
        {
            return charCount * 2;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            return (byteCount + 1) / 2;
        }
    }
}
