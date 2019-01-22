using System.IO;
using Nancy;

namespace TestMetaServer
{
    public class CustomRootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}