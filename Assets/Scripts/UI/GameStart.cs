using TMPro;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public GameObject OpenUI;
    public TextMeshProUGUI OpenText;

    private bool isStarted = false;

    void Update()
    {
        if (isStarted)
            return;

        if (Input.GetMouseButtonDown(0) && isStarted == false)
        {
            isStarted = true;

            OpenUI.SetActive(true);
            OpenText.gameObject.SetActive(false);
        }
    }
}