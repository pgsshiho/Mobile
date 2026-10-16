using UnityEngine;

public class OpenUI : MonoBehaviour
{
    [SerializeField] private GameObject[] uiObjects;

    // 지정된 모든 UI 활성화
    public void OpenAll()
    {
        if (uiObjects == null) return;

        foreach (GameObject ui in uiObjects)
        {
            if (ui == null) continue;
            ui.SetActive(true);
            PartymenuSpawn.NotifyPanelState(ui);
        }
    }

    // 지정된 모든 UI 비활성화
    public void CloseAll()
    {
        if (uiObjects == null) return;

        foreach (GameObject ui in uiObjects)
        {
            if (ui == null) continue;
            ui.SetActive(false);
            PartymenuSpawn.NotifyPanelState(ui);
        }
    }

    // 현재 상태를 반전 (켜져있으면 끄고, 꺼져있으면 켬)
    public void ToggleAll()
    {
        if (uiObjects == null) return;

        foreach (GameObject ui in uiObjects)
        {
            if (ui == null) continue;
            ui.SetActive(!ui.activeSelf);
            PartymenuSpawn.NotifyPanelState(ui);
        }
    }

    public void OnSetting()
    {
        Debug.Log("OnSetting called");
        if (AudioManager.instance != null)
            AudioManager.instance.OnChild();
        else
            Debug.LogWarning("AudioManager instance is null. Cannot call OnChild().");
    }
    public void OffSetting()
    {
        Debug.Log("OffSetting called");
        if (AudioManager.instance != null)
            AudioManager.instance.OffChild();
        else
            Debug.LogWarning("AudioManager instance is null. Cannot call OffChild().");

    }
}
