using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageSettings : MonoBehaviour
{
    public TMP_Dropdown LanguageDropdown;

    private void Start()
    {
        // 유니티가 기본으로 제공하는 드롭다운 요소들을 제거합니다.
        LanguageDropdown.ClearOptions();

        // 드롭다운에 언어를 추가합니다.
        LanguageDropdown.AddOptions(LocalizationSettings.AvailableLocales.Locales.ConvertAll(locale => locale.LocaleName));

        // 드롭 다운에 선택된 언어를 설정합니다.
        LanguageDropdown.value = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);

        // 드롭다운의 값이 변경되면 로컬라이징을 변경합니다.
        LanguageDropdown.onValueChanged.AddListener(ChangeLocale);
    }

    /// <summary>
    /// 로컬라이징을 변경합니다.
    /// </summary>
    /// <param name="index">Edit->Project Settings->Localization에 설정한 언어 순서의 Index입니다.</param>
    public void ChangeLocale(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        GlobalSaveData settings = Save.GetGlobalSaveData();
        Save.UpdateGlobalSettings(
            index,
            settings.masterVolume,
            settings.bgmVolume,
            settings.sfxVolume);
    }
}
