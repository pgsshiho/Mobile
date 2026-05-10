using UnityEngine;

public class GameStart : MonoBehaviour 
{ 

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SceneChanger.BG("Factory");
            Debug.Log("게임이 시작되었습니다!");
        }
    }
}
