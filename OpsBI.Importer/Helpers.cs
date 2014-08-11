using System.IO;
using System.Reflection;

namespace OpsBI.Importer
{
    class Helpers
    {
        public static string GetEmbeddedJson(Assembly assembly, string embeddedResourcePath)
        {
            if (!embeddedResourcePath.StartsWith(".")) embeddedResourcePath = "." + embeddedResourcePath;

            using (var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + embeddedResourcePath))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
