using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace NetTopologySuite.IO.Streams.CloudStorage.Test
{
    public class TempFileCloudUploader : IDisposable
    {
        public const string TestingContainerName = "ntsunittests";//https://msdn.microsoft.com/library/azure/dd135715.aspx

        public TempFileCloudUploader(string fileName, byte[] data)
        {
            Path = fileName;
            GetBlobReference(fileName).UploadFromByteArray(data, 0, data.Length);
        }

        private static CloudBlockBlob GetBlobReference(string name)
        {
            var dir = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudBlobClient()
               .GetContainerReference(TestingContainerName);

            dir.CreateIfNotExists();


            var blob = dir.GetBlockBlobReference(name);


            return blob;
        }

        public TempFileCloudUploader(string fileName, string data)
        {
            Path = fileName;
            GetBlobReference(fileName).UploadText(data);
        }

        ~TempFileCloudUploader()
        {
            InternalDispose();
        }

        public void Dispose()
        {
            InternalDispose();
            GC.SuppressFinalize(this);
        }

        private void InternalDispose()
        {
            try
            {
                GetBlobReference(Path).DeleteIfExists();
            }
            catch { }
        }

        public string Path { get; private set; }
    }
}
