using UnityEngine;

public class GameStart : MonoBehaviour
{
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchPressureSupported)
        {
            if(Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if(touch.phase == TouchPhase.Began)
                {
                    SceneChanger.BG("Factory");
                    Debug.Log("게임이 시작되었습니다!");
                }
            }
        }
    }
}
