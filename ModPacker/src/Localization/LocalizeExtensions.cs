namespace ModPacker.Localization;

public static class LocalizeExtensions
{
    public static string Translate(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return "";
        return LocalizeManager.GetInstance.GetTranslation(str);
    }
    public static string Translate(this string str, int langid)
    {
        if (string.IsNullOrEmpty(str))
            return "";
        return LocalizeManager.GetInstance.GetTranslation(str, langid);
    }
}