using System;
using System.IO.IsolatedStorage;
using System.Resources;
using System.Windows;
using GisSharpBlog.NetTopologySuite.Data;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
namespace NetTopologySuite.Silverlight.Test
{
    [TestClass]
    public class DataTests
    {
        [TestMethod]
        public void TestValueFactory()
        {
            IValueFactory factory = new ValueFactory();
            Assert.IsInstanceOfType(factory.CreateValue(typeof(int), (object)1), typeof(IValue<int>));
            Assert.IsInstanceOfType(factory.CreateValue<int>((object)1), typeof(IValue<int>));
            Assert.IsInstanceOfType(factory.CreateValue<int>((object)"1"), typeof(IValue<int>));
            Assert.IsInstanceOfType(factory.CreateValue<double>((object)1), typeof(IValue<double>));
        }


        void EnsureFile(string fileName, string sourceUri)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isf.FileExists(fileName))
                {
                    using (var f = isf.CreateFile(fileName))
                    {
                        using (var res = Application.GetResourceStream(new Uri(sourceUri, UriKind.Relative)).Stream)
                        {
                            if (isf.AvailableFreeSpace < res.Length)
                            {
                                isf.IncreaseQuotaTo(isf.Quota + 10000000);

                            }

                            res.CopyTo(f);
                        }
                        f.Flush();
                        f.Close();
                    }
                }
            }
        }

        public void EnsureFilesExistInIsolatedStorage()
        {
            EnsureFile("world.shp", "world.shp");
            EnsureFile("world.dbf", "world.dbf");
            EnsureFile("world.shx", "world.shx");
        }

        [TestMethod]
        public void TestShapefile()
        {
            EnsureFilesExistInIsolatedStorage();

            IMemoryRecordSet memoryRecordSet =
                Shapefile.CreateDataReader(@"world.shp", new GeometryFactory()).ToInMemorySet();

            memoryRecordSet.Where(a => a.GetValue<int>(a.Schema.IdProperty).Value == 1);
        }
    }
}