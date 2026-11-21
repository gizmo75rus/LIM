using System;
using System.IO;

namespace AstmDriver.Commons
{
    public class LocalStorage
    {
        string _path = string.Empty;

        public LocalStorage()
        {
            _path = Path.Combine(Environment.CurrentDirectory, "local_storage.json");
            if (File.Exists(_path))
            {

            }
        }
    }
}
