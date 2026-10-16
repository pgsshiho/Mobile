using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 각 지역 씬의 Party Setting Button과 Party Setting UI를 자동 연결한다.
/// 기존 OpenUI가 붙어 있는 씬은 기존 연결을 사용하고, 누락된 씬만 보완한다.
/// </summary>
public static class PartySettingUIBootstrap
{
    private const string ButtonName = "Party Setting Button";
    private const string PanelName = "Party Setting UI";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterInitialScene()
    {
        SceneManager.sceneLoaded += (_, __) => RegisterCurrentScene();
        RegisterCurrentScene();
    }

    private static void RegisterCurrentScene()
    {
        GameObject buttonObject = FindSceneObject(ButtonName);
        GameObject panelObject = FindSceneObject(PanelName);
        if (buttonObject == null || panelObject == null)
            return;

        // Forest 씬처럼 OpenUI가 이미 정상 연결된 버튼은 기존 이벤트가 패널을 연다.
        if (buttonObject.GetComponent<OpenUI>() != null)
            return;

        Button button = buttonObject.GetComponent<Button>();
        if (button == null || button.GetComponent<PartySettingButtonHook>() != null)
            return;

        PartySettingButtonHook hook = buttonObject.AddComponent<PartySettingButtonHook>();
        hook.Initialize(panelObject);
    }

    private static GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (candidate.name == objectName && candidate.scene.IsValid() && candidate.scene.isLoaded)
                return candidate;
        }

        return null;
    }
}

/// <summary>런타임에서 버튼 클릭을 패널 열기와 UI 프리팹 생성에 연결한다.</summary>
public class PartySettingButtonHook : MonoBehaviour
{
    private GameObject partyPanel;

    public void Initialize(GameObject panel)
    {
        partyPanel = panel;
        GetComponent<Button>().onClick.AddListener(TogglePartySetting);
    }

    private void TogglePartySetting()
    {
        if (partyPanel == null)
            return;

        partyPanel.SetActive(!partyPanel.activeSelf);
        PartymenuSpawn.NotifyPanelState(partyPanel);
    }
}
