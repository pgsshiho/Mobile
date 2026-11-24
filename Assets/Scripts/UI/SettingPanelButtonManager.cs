using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingPanelButtonManager : MonoBehaviour
{
    public static SettingPanelButtonManager instance;
    public GameObject MainmenuButton;
    public GameObject LeftButton;
    public GameObject QuitPanel;
    public TextMeshProUGUI QuitText;
    private void OnEnable()
    {
        ButtonSetting();
    }
    private void OnDisable()
    {
        ButtonSetting();
    }
    void Start()
    {
        ButtonSetting();
    }


    public void ButtonSetting()
    {
        if(SceneManager.GetActiveScene().name == "Mainmenu")
        {
            MainmenuButton.SetActive(false);
            LeftButton.transform.localPosition = new Vector3(0, LeftButton.transform.localPosition.y, LeftButton.transform.localPosition.z);
        }
        else
        {
            MainmenuButton.SetActive(true);
            LeftButton.transform.localPosition = new Vector3(300, LeftButton.transform.localPosition.y, LeftButton.transform.localPosition.z);
        }
    }
    public void GoMainmeni()
    {
        SceneChanger.BG("Mainmenu");
    }
    public void GameQuitAsk()
    {
        QuitPanel.SetActive(true);
        QuitText.text = LocalizationManager.GetLocalizedString("QuitAsk");
    }
    public void GameQuit()
    {
        Save.instance.SaveGame();
        Application.Quit();
    }
    public void Cancel()
    {
        QuitPanel.SetActive(false);
    }
}
