using TMPro;
using UnityEngine;
using DG.Tweening;
public class BrightText : MonoBehaviour
{
    public TextMeshProUGUI textMesh; // 만약 일반 UI Text라면 'Text'로 변경

    void Start()
    {
        textMesh.DOFade(0.2f, 1.0f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine); // 부드럽게 가속/감속
    }
    public void StopBlinking()
    {
        textMesh.DOKill(); // 애니메이션 중지
        textMesh.alpha = 1f; // 원래 밝기로 복구
    }
}
