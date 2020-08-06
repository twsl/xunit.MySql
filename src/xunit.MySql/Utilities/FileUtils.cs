using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Xunit.MySql.Utilities
{
    internal class FileUtils
    {
        public static string GetBaseDir() => new DirectoryInfo(Directory.GetCurrentDirectory()).FullName;

        public static void WriteFileToFolder(string path, string partialNamespace, string name) => File.WriteAllBytes(Path.Combine(path, name),
                                                                                                                      GetResourceFile(partialNamespace, name));

        public static byte[] GetResourceFile(string partialNamespace, string name) => GetResourceFileAsync(partialNamespace, name).Result;

        public static async Task<byte[]> GetResourceFileAsync(string partialNamespace, string name)
        {
            var assembly = typeof(FileUtils).GetTypeInfo().Assembly;
            string fullname = $"Xunit.MySql.Resources.{partialNamespace}.{name}";
            using var stream = assembly.GetManifestResourceStream(fullname);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            return ms.ToArray();
        }
    }
}
