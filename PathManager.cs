using System;
using System.IO;
using System.Reflection;

class PathManager
{
    public static string GetIniFilename()
    {
        var assemblyFilename = Assembly.GetExecutingAssembly().Location;
        return Path.ChangeExtension(assemblyFilename, ".ini");
    }
}