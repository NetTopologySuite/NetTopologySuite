using System;
using System.IO;
using System.Text;
using NetTopologySuite.IO.Streams;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.Streams
{
    [TestFixture]
    public class ByteStreamProviderFixture
    {
        [TestCase("This is sample text", 0)]
        [TestCase("Dies sind deutsche Umlaute: Ää. Öö, Üü, ß", 0)]
        [TestCase("Dies sind deutsche Umlaute: Ää. Öö, Üü, ß", 850)]
        [TestCase("Dies sind deutsche Umlaute: Ää. Öö, Üü, ß", 437)]
        public void TestConstructorText(string constructorText, int codepage)
        {
            var encoding = codepage == 0 ? Encoding.Default : Encoding.GetEncoding(codepage); 

            var bsp = new ByteStreamProvider("Test", constructorText, encoding);
            Assert.That(bsp.UnderlyingStreamIsReadonly, Is.True);
            Assert.That(bsp.Length, Is.EqualTo(constructorText.Length));
            Assert.That(bsp.Length == bsp.MaxLength, Is.True);

            using (var streamreader = new StreamReader(bsp.OpenRead(), encoding))
            {
                var streamText = streamreader.ReadToEnd();
                Assert.That(streamText, Is.EqualTo(constructorText));
            }
        }

        [TestCase(50, 50, true)]
        [TestCase(50, 100, true)]
        [TestCase(50, 50, false)]
        [TestCase(50, 100, false)]
        public void TestConstructor(int length, int maxLength, bool @readonly)
        {
            var bsp = new ByteStreamProvider("Test", CreateData(length), maxLength, @readonly);
            Assert.That(bsp.UnderlyingStreamIsReadonly, Is.EqualTo(@readonly));
            Assert.That(bsp.Length, Is.EqualTo(length));
            Assert.That(bsp.MaxLength, Is.EqualTo(maxLength));

            using (var ms = (MemoryStream)bsp.OpenRead())
            {
                var data = ms.ToArray();
                Assert.That(data, Is.Not.Null);
                Assert.That(data.Length, Is.EqualTo(length));
                for (var i = 0; i < length; i++)
                    Assert.That(data[i], Is.EqualTo(bsp.Buffer[i]));
            }

            try
            {
                using (var ms = (MemoryStream)bsp.OpenWrite(false))
                {
                    var sw = new BinaryWriter(ms);
                    sw.BaseStream.Position = 50;
                    for (var i = 0; i < 10; i++)
                        sw.Write((byte)i);
                    sw.Flush();
                    Assert.That(ms.Length, Is.EqualTo(length+10));
                    Assert.That(bsp.Length, Is.EqualTo(length+10));
                    Assert.That(bsp.Buffer[59], Is.EqualTo(9));
                }
            }
            catch (Exception ex)
            {
                if (ex is AssertionException)
                    throw;

                if (!@readonly)
                {
                    Assert.That(ex, Is.TypeOf(typeof(NotSupportedException)));
                    Assert.That(length, Is.EqualTo(maxLength));
                }
            }
        }

        private static byte[] CreateData(int length)
        {
            var rnd = new Random();

            var res = new byte[length];
            for (var i = 0; i < length; i++)
                res[i] = (byte) rnd.Next(0, 255);
            return res;
        }
    }
}