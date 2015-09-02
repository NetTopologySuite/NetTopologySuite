using System;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using NetTopologySuite.IO.Streams;

namespace NetTopologySuite.IO.Shapefile.Extended.CloudStorage
{
    public class CloudStreamProvider : IStreamProvider
    {
        public CloudStreamProvider(CloudBlobContainer container, string path)
        {
            if(path == null) 
                throw new ArgumentNullException(nameof(path));

            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(nameof(path));

            if (container == null)
                throw new ArgumentNullException(nameof(container));

            Container = container;
            Path = path;

            if (!Container.GetBlobReference(Path).Exists())
                throw new FileNotFoundException();

            BlobType = container.GetBlobReference(path).BlobType;
        }

        private BlobType BlobType { get; }

        private string Path { get; }

        private CloudBlobContainer Container { get; }


        public bool UnderlyingStreamIsReadonly
        {
            get { return true; }
        }

        public Stream OpenRead()
        {
            return BlobType == BlobType.PageBlob
                ? Container.GetPageBlobReference(Path).OpenRead()
                : Container.GetBlobReference(Path).OpenRead();
        }

        public Stream OpenWrite(bool truncate)
        {
            throw new NotImplementedException();

            //todo:jd writable streams are possible for page blobs - however they need to be initialized with a size. 
        }

        public string Kind
        {
            get { return "CloudStream"; }
        }
    }
}