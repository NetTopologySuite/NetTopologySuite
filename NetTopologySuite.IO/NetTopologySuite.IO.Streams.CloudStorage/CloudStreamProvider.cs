using System;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO.Streams
{
    public class CloudStreamProvider : IStreamProvider
    {
        public CloudStreamProvider(CloudBlobContainer container, string kind,  string path)
        {
            if(path == null) 
                throw new ArgumentNullException("path");

            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("path");

            if (container == null)
                throw new ArgumentNullException("container");

            Container = container;
            Kind = kind;
            Path = path;

            if (!Container.GetBlobReference(Path).Exists())
                throw new FileNotFoundException();

            BlobType = container.GetBlobReference(path).BlobType;
        }

        private BlobType BlobType { get; set; }

        private string Path { get; set; }

        private CloudBlobContainer Container { get; set; }


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

        public string Kind { get; private set; }
    }
}