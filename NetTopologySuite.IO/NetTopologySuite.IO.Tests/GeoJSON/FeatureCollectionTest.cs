using System.Collections.ObjectModel;
using NUnit.Framework;
using NetTopologySuite.Features;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    ///<summary>
    ///    This is a test class for FeatureCollectionTest and is intended
    ///    to contain all FeatureCollectionTest Unit Tests
    ///</summary>
    [TestFixture]
    public class FeatureCollectionTest
    {      
        ///<summary>
        ///    A test for FeatureCollection Constructor
        ///</summary>
        [Test]
        public void FeatureCollectionConstructorTest()
        {
            FeatureCollection target = new FeatureCollection();
            Assert.IsInstanceOfType(typeof(FeatureCollection), target);
        }

        ///<summary>
        ///    A test for FeatureCollection Constructor
        ///</summary>
        [Test]
        public void FeatureCollectionConstructorTest1()
        {
            Collection<IFeature> features = new Collection<IFeature> {new Feature()};
            FeatureCollection target = new FeatureCollection(features);
            Assert.IsInstanceOfType(typeof(FeatureCollection), target);
            Assert.AreEqual(features, target.Features);
        }

        ///<summary>
        ///    A test for Add
        ///</summary>
        [Test]
        public void AddTest()
        {
            FeatureCollection target = new FeatureCollection(); 
            IFeature feature = new Feature();
            target.Add(feature);
            Assert.AreEqual(feature, target.Features[0]);
        }

        ///<summary>
        ///    A test for Remove
        ///</summary>
        [Test]
        public void RemoveTest()
        {
            IFeature feature = new Feature();
            FeatureCollection target = new FeatureCollection(new Collection<IFeature> { feature }); 
            const bool expected = true;
            bool actual = target.Remove(feature);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(0, target.Count);
        }

        ///<summary>
        ///    A test for RemoveAt
        ///</summary>
        [Test]
        public void RemoveAtTest()
        {
            IFeature feature = new Feature();
            FeatureCollection target = new FeatureCollection(new Collection<IFeature> { feature }); 
            const int index = 0;
            target.RemoveAt(index);
            Assert.AreEqual(0, target.Count);
        }

        ///<summary>
        ///    A test for Count
        ///</summary>
        [Test]
        public void CountTest()
        {
            IFeature feature = new Feature();
            FeatureCollection target = new FeatureCollection(new Collection<IFeature> { feature });
            Assert.AreEqual(1, target.Count);
        }

        ///<summary>
        ///    A test for Item
        ///</summary>
        [Test]
        public void ItemTest()
        {
            IFeature feature = new Feature();
            FeatureCollection target = new FeatureCollection(new Collection<IFeature> { feature });
            IFeature actual = target[0];
            Assert.AreSame(feature, actual);
        }

        ///<summary>
        ///    A test for Type
        ///</summary>
        [Test]
        public void TypeTest()
        {
            FeatureCollection target = new FeatureCollection();
            Assert.AreEqual("FeatureCollection", target.Type);
        }
    }
}