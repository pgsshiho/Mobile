using TMPro;
using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트를 다루기 위해 필수 추가

public class ButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Color Settings")]
    public Color HighlightColor = Color.yellow; // 하이라이트 색상
    public Color NormalColor = Color.white;    // 기본 색상

    private TextMeshProUGUI targetText;

    private void Awake()
    {
        // Start보다 빠른 Awake 시점에 텍스트 컴포넌트 캐싱
        targetText = GetComponentInChildren<TextMeshProUGUI>();

        if (targetText == null)
        {
            Debug.LogWarning($"{gameObject.name}의 자식 오브젝트에서 TextMeshProUGUI를 찾을 수 없습니다.");
        }
    }

    // 마우스를 올렸을 때 호출
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetText != null)
        {
            targetText.color = HighlightColor;
        }
    }

    // 마우스가 벗어났을 때 호출
    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetText != null)
        {
            targetText.color = NormalColor;
        }
    }
}