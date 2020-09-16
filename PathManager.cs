using System;
using System.IO;
using System.Reflection;

class PathManager
{
    public static string GetIniFilename()
    {
        var assemblyFilename = new Uri(Assembly.GetExecutingAssembly().CodeBase ?? string.Empty)
            .LocalPath;
        return Path.ChangeExtension(assemblyFilename, ".ini");
    }
}