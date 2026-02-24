using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform;

namespace ModPacker.Localization;

public static class LocalizationHealer
{
    public static void CreateDefaultConfiguration()
    {
        Console.WriteLine("CREATING CONFIGURATION");
        string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        
        string content = "";
        using (var stream = AssetLoader.Open(GetResourceUri("src/Assets/default.json")))
        using (var reader = new StreamReader(stream))
        {
            content = reader.ReadToEnd();
        }

        if (!Directory.Exists(dataDir))
            Directory.CreateDirectory(dataDir);
        
        
        Console.WriteLine("WRITING CONFIGURATION");
        File.WriteAllText(Path.Combine(dataDir, "langs.json"), content);
        Console.WriteLine("DONE");
        LocalizeManager.GetInstance.LoadConfiguration();
    }
    
    private static Uri GetResourceUri(string relativePath)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        // Убираем лишние слеши и точки
        var cleanPath = relativePath.Replace('\\', '/').TrimStart('/');
        return new Uri($"avares://{assemblyName}/{cleanPath}");
    }
}