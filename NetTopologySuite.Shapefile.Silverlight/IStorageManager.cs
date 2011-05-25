using System;
using System.Collections.Generic;
using System.IO;

namespace NetTopologySuite.Shapefile
{
    public interface IStorageManager : IDisposable
    {
        bool FileExists(string path);
        Stream OpenRead(string path);
        Stream Open(string path, FileMode mode, FileAccess access, FileShare share);

        void FileDelete(string path);


        string ReadAllText(string path);
    }
}