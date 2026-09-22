using UnityEngine;

public class BackButtonUI : MonoBehaviour
{
    public GameObject[] forbackbuttonUI;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            foreach (GameObject ui in forbackbuttonUI)
            {
                if (ui != null) ui.SetActive(false);
            }
        }
    }
    public void CloseUI()
    {
        foreach (GameObject ui in forbackbuttonUI)
        {
            if (ui != null) ui.SetActive(false);
        }
    }
    public void ToggleUI()
    {
        foreach (GameObject ui in forbackbuttonUI)
        {
            if (ui != null) ui.SetActive(!ui.activeSelf);
        }
    }
}
