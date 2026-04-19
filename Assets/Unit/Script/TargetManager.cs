using System.Collections;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    public static TargetManager Instance;
    void Awake() => Instance = this;

    // 플레이어가 타겟을 선택할 때까지 대기하는 코루틴용 함수
    public IEnumerator WaitForPlayerTarget(System.Action<GameObject> onTargetSelected)
    {
        GameObject selected = null;
        while (selected == null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    ITakeDamage target = hit.collider.GetComponent<ITakeDamage>();
                    if (target != null) // ITakeDamage를 가진 타겟인가?
                    {
                        selected = hit.collider.gameObject;
                        // 여기서 범위 체크 로직 추가 가능 (예: gridPosition 확인)
                    }
                }
            }
            yield return null;
        }
        onTargetSelected?.Invoke(selected);
    }
}