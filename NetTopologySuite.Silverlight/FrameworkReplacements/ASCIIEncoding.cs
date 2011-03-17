using System;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.FrameworkReplacements
{
    public class ASCIIEncoding : Encoding
    {
        public override int GetByteCount(char[] chars, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            throw new NotImplementedException();
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            throw new NotImplementedException();
        }

        public override int GetMaxByteCount(int charCount)
        {
            throw new NotImplementedException();
        }

        public override int GetMaxCharCount(int byteCount)
        {
            throw new NotImplementedException();
        }

    }
}