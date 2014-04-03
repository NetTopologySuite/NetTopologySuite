using System;
using System.IO;
using System.Linq;
using NetTopologySuite.Features;
using NetTopologySuite.IO.ShapeFile.Extended;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.ShapeFile.Extended
{
    /// <summary>
    /// Summary description for DbfFileReaderTests
    /// </summary>
    [TestFixture]
    public class DbaseReaderTests
    {
        private static readonly DateTime DATE_SAVED_IN_DBF = new DateTime(1990, 1, 1);

        private DbaseReader m_Reader;
        private TempFileWriter m_TmpFile;

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_SendNullPath_ShouldThrowException()
        {
            // Act.
            m_Reader = new DbaseReader(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_SendEmptyString_ShouldThrowException()
        {
            // Act.
            m_Reader = new DbaseReader(string.Empty);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_SendWhitespaceString_ShouldThrowException()
        {
            // Act.
            m_Reader = new DbaseReader("    \t  ");
        }

        [Test]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Ctor_SendNonExistantPath_ShouldThrowException()
        {
            // Act.
            m_Reader = new DbaseReader(@"C:\this\is\sheker\path\should\never\exist\on\ur\pc");
        }

        [Test]
        public void Ctor_SendValidParameters_ShouldReturnNotNull()
        {
            // Arrange
            m_TmpFile = new TempFileWriter("data.dbf", DbfFiles.Read("line_ed50_geo"));

            // Act.
            m_Reader = new DbaseReader(m_TmpFile.Path);

            // Assert.
            Assert.IsNotNull(m_Reader);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadEntry_SendNegativeIndex_ShouldThrowException()
        {
            // Arrange
            m_TmpFile = new TempFileWriter("data.dbf", DbfFiles.Read("point_ed50_geo"));
            m_Reader = new DbaseReader(m_TmpFile.Path);

            // Act.
            m_Reader.ReadEntry(-1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ReadEntry_SendOutOfBoundIndex_ShouldThrowException()
        {
            // Arrange
            m_TmpFile = new TempFileWriter("data.dbf", DbfFiles.Read("point_ed50_geo"));
            m_Reader = new DbaseReader(m_TmpFile.Path);

            // Act.
            m_Reader.ReadEntry(3);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReadEntry_TryReadAfterDisposed_ShouldThrowException()
        {
            // Arrange
            m_TmpFile = new TempFileWriter("data.dbf", DbfFiles.Read("point_ed50_geo"));
            m_Reader = new DbaseReader(m_TmpFile.Path);

            m_Reader.Dispose();

            // Act.
            m_Reader.ReadEntry(1);
        }

        [Test]
        public void ReadEntry_ReadEntryValues_ShoudReturnCorrectValues()
        {
            // Arrange
            m_TmpFile = new TempFileWriter("data.dbf", DbfFiles.Read("point_ed50_geo"));
            m_Reader = new DbaseReader(m_TmpFile.Path);

            var expectedTable = new
            {
                Ids = new double[] 
				{ 
					3, 2, 1
				},
                Strings = new string[] 
				{ 
					"str3", "str2", "str1"
				},
                WholeNums = new double[] 
				{ 
					3, 2, 1
				},
                DecNums = new double[] 
				{
					3, 2, 1
				},
            };

            // Act.
            IAttributesTable[] results = new IAttributesTable[]
			{
				m_Reader.ReadEntry(0),
				m_Reader.ReadEntry(1),
				m_Reader.ReadEntry(2)
			};

            // Assert.
            int currResIndex = 0;
            foreach (IAttributesTable res in results)
            {
                object id = res["id"];
                object str = res["str"];
                object wholeNum = res["wholeNum"];
                object decNum = res["decNum"];
                object date = res["dt"];

                Assert.IsNotNull(id);
                Assert.IsNotNull(str);
                Assert.IsNotNull(wholeNum);
                Assert.IsNotNull(decNum);
                Assert.IsNotNull(date);

                Assert.IsInstanceOf<double>(id);
                Assert.IsInstanceOf<string>(str);
                Assert.IsInstanceOf<double>(wholeNum);
                Assert.IsInstanceOf<double>(decNum);
                Assert.IsInstanceOf<DateTime>(date);

                Assert.AreEqual(id, expectedTable.Ids[currResIndex]);
                Assert.AreEqual(str, expectedTable.Strings[currResIndex]);
                Assert.AreEqual(wholeNum, expectedTable.WholeNums[currResIndex]);
                Assert.AreEqual(decNum, expectedTable.DecNums[currResIndex]);
                Assert.AreEqual(date, DATE_SAVED_IN_DBF);

                currResIndex++;
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ReadEntry_ReadNonExistantKeyFromEntry_ShoudReturnCorrectValues()
        {
            // Arrange
            m_TmpFile = new TempFileWriter("data.dbf", DbfFiles.Read("point_ed50_geo"));
            m_Reader = new DbaseReader(m_TmpFile.Path);

            IAttributesTable results = m_Reader.ReadEntry(0);

            // Act.
            object a = results["a"];
        }

        [Test]
        public void ForEachIteration_ReadEntryValues_ShoudReturnCorrectValues()
        {
            // Arrange
            m_TmpFile = new TempFileWriter("data.dbf", DbfFiles.Read("point_ed50_geo"));
            m_Reader = new DbaseReader(m_TmpFile.Path);

            var expectedTable = new
            {
                Ids = new double[] 
				{ 
					3, 2, 1
				},
                Strings = new string[] 
				{ 
					"str3", "str2", "str1"
				},
                WholeNums = new double[] 
				{ 
					3, 2, 1
				},
                DecNums = new double[] 
				{
					3, 2, 1
				},
            };

            // Act.
            IAttributesTable[] results = m_Reader.ToArray();

            Assert.AreEqual(results.Length, 3);

            // Assert.
            int currResIndex = 0;
            foreach (IAttributesTable res in results)
            {
                object id = res["id"];
                object str = res["str"];
                object wholeNum = res["wholeNum"];
                object decNum = res["decNum"];
                object date = res["dt"];

                Assert.IsNotNull(id);
                Assert.IsNotNull(str);
                Assert.IsNotNull(wholeNum);
                Assert.IsNotNull(decNum);
                Assert.IsNotNull(date);

                Assert.IsInstanceOf<double>(id);
                Assert.IsInstanceOf<string>(str);
                Assert.IsInstanceOf<double>(wholeNum);
                Assert.IsInstanceOf<double>(decNum);
                Assert.IsInstanceOf<DateTime>(date);

                Assert.AreEqual(id, expectedTable.Ids[currResIndex]);
                Assert.AreEqual(str, expectedTable.Strings[currResIndex]);
                Assert.AreEqual(wholeNum, expectedTable.WholeNums[currResIndex]);
                Assert.AreEqual(decNum, expectedTable.DecNums[currResIndex]);
                Assert.AreEqual(date, DATE_SAVED_IN_DBF);

                currResIndex++;
            }
        }

        [TearDown]
        public void TestCleanup()
        {
            if (m_Reader != null)
            {
                m_Reader.Dispose();
                m_Reader = null;
            }

            if (m_TmpFile != null)
            {
                m_TmpFile.Dispose();
                m_TmpFile = null;
            }
        }
    }

    static class DbfFiles
    {
        public static byte[] Read(string filename)
        {
            string basedir = AppDomain.CurrentDomain.BaseDirectory;
            string format = String.Format("..{0}..{0}..{0}NetTopologySuite.Samples.Shapefiles", Path.DirectorySeparatorChar);
            String folder = Path.Combine(basedir, format);
            String file = Path.ChangeExtension(filename, ".dbf");
            String path = Path.Combine(folder, file);
            Assert.That(File.Exists(path), Is.True);
            return File.ReadAllBytes(path);
        }
    }
}
