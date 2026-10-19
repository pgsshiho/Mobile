
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class LocalizationManager
{
    /// <summary>
    /// 문자열을 Localization Table의 Key로 사용해 번역된 문자열을 반환한다.
    /// </summary>
    public static string GetLocalizedString(string key)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        var table = LocalizationSettings.StringDatabase.GetTable("En");

        if (table == null)
        {
            Debug.LogWarning("Localization Table 'En'를 찾을 수 없습니다.");
            return key;
        }

        var entry = table.GetEntry(key);

        if (entry == null)
        {
            Debug.LogWarning($"Localization Key '{key}'를 찾을 수 없습니다.");
            return key;
        }

        return entry.GetLocalizedString();
    }
    public static string GetLocalizedString(string key, params object[] arguments)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        var table = LocalizationSettings.StringDatabase.GetTable("En");

        if (table == null)
        {
            Debug.LogWarning("Localization Table 'En'를 찾을 수 없습니다.");
            return key;
        }

        var entry = table.GetEntry(key);

        if (entry == null)
        {
            Debug.LogWarning($"Localization Key '{key}'를 찾을 수 없습니다.");
            return key;
        }

        return entry.GetLocalizedString(arguments);
    }
}
