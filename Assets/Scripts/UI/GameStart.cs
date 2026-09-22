using UnityEngine;

/// <summary>
/// 메인 메뉴 진입점 스크립트.
/// MainMenuController를 통해 이어하기 및 새로 시작(챕터 선택)을 관리합니다.
/// </summary>
public class GameStart : MonoBehaviour 
{ 
    private void Start()
    {
        // MainMenuController가 없으면 현재 오브젝트에 추가
        if (GetComponent<MainMenuController>() == null)
        {
            gameObject.AddComponent<MainMenuController>();
        }
    }
}
