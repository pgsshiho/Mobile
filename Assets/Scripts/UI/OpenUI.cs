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
            if (ui != null) ui.SetActive(true);
        }
    }

    // 지정된 모든 UI 비활성화
    public void CloseAll()
    {
        if (uiObjects == null) return;

        foreach (GameObject ui in uiObjects)
        {
            if (ui != null) ui.SetActive(false);
        }
    }

    // 현재 상태를 반전 (켜져있으면 끄고, 꺼져있으면 켬)
    public void ToggleAll()
    {
        if (uiObjects == null) return;

        foreach (GameObject ui in uiObjects)
        {
            if (ui != null) ui.SetActive(!ui.activeSelf);
        }
    }
}