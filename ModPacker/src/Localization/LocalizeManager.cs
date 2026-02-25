using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ModPacker.Localization;

public class LocalizeManager
{
    private List<string> Languages = new();
    private List<KeyWord> Localizations = new();

    private int _curLangId = 1;
    public int currentLangID
    {
        get { return _curLangId; }
        
        set
        {
            if(value <= Languages.Count-1 && value >= 0)
            {
                Console.WriteLine("SETTED TO " + value);
                _curLangId = value;
            }
        }
    }
    private string cfgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data\\langs.json");
    
    private static LocalizeManager? _instance;
    private static object _lock = new object();

    public static LocalizeManager GetInstance
    {
        get
        {
            if(_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new LocalizeManager();
                    }
                }
            }

            return _instance;
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public void SaveConfiguration()
    {
        var options1 = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true
        };
        
        string json = JsonSerializer.Serialize(new LangSaveConfig
        {
            Localizations = Localizations,
            Languages = Languages,
            currentLangID = this.currentLangID
        }, options1);
        
        File.WriteAllText(cfgPath, json);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public void LoadConfiguration()
    {
        string json = File.ReadAllText(cfgPath);
        
        if(!String.IsNullOrEmpty(json)) {
            try
            {
                Console.WriteLine("DESERIALIZING CONFIGURATION");
                LangSaveConfig? config = JsonSerializer.Deserialize<LangSaveConfig>(json);
    
                if (config != null)
                {
                    Localizations = config.Value.Localizations;
                    Languages = config.Value.Languages;
                    _curLangId = config.Value.currentLangID;
                
                    Console.WriteLine(_curLangId);
                }
            }
            catch (Exception e)
            {
                LocalizationHealer.CreateDefaultConfiguration();
            }
            
        }
        else
        {
            LocalizationHealer.CreateDefaultConfiguration();
        }
    }

    public string GetTranslation(string keyword,int langID) //Exact lang
    {
        try
        {
        return Localizations.Where(x => x.Key == keyword).First().Translations[langID];
        }
        catch (Exception e)
        {
            LocalizationHealer.CreateDefaultConfiguration();
            return Localizations.Where(x => x.Key == keyword).First().Translations[langID];
        }
    }

    public string GetTranslation(string keyword) //Curent lang
    {
        try
        {
            return Localizations.Where(x => x.Key == keyword).First().Translations[_curLangId];
        }
        catch (Exception e)
        {
            LocalizationHealer.CreateDefaultConfiguration();
            return Localizations.Where(x => x.Key == keyword).First().Translations[_curLangId];
        }
    }
    
}