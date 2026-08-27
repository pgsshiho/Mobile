using UnityEngine;

public class GameStart : MonoBehaviour 
{ 
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            string sceneToLoad = Save.GetSavedScene("Factory");
            if (Save.instance != null)
            {
                Save.instance.LoadGame();
            }
            SceneChanger.BG(sceneToLoad);
            Debug.Log("게임이 시작되었습니다!");
        }
    }
}
