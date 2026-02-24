using System.Collections.Generic;

namespace ModPacker.Localization;

[System.Serializable]
public struct LangSaveConfig
{
    public List<string> Languages { get; set; }
    public List<KeyWord> Localizations { get; set; }
    public int currentLangID { get; set; }
}