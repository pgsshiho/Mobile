using System;
using DG.Tweening;
using UnityEngine;

public class FocusManager : MonoBehaviour
{
    [Header("기본 연출 설정")]
    public float duration = 1.0f;
    public float targetScaleMultiplier = 2.5f;
    [Tooltip("3D 월드 오브젝트 포커스 시 카메라와의 거리")]
    public float distanceFromCamera = 5f;

    public bool IsTweening { get; private set; } = false;

    // =========================================================
    // [C# Action] 외부에서 FocusManager를 호출할 수 있는 창구
    // =========================================================
    public static Action<GameObject> RequestFocusIn;
    public static Action RequestFocusOut;
    public static Action<GameObject> RequestToggleFocus;

    private Camera mainCamera;
    private FocusableObject currentFocusedTarget;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    // 오브젝트가 활성화될 때 이벤트 연결
    private void OnEnable()
    {
        RequestFocusIn += FocusIn;
        RequestFocusOut += FocusOut;
        RequestToggleFocus += ToggleFocus;
    }

    // 오브젝트가 비활성화될 때 이벤트 해제 (메모리 누수 방지)
    private void OnDisable()
    {
        RequestFocusIn -= FocusIn;
        RequestFocusOut -= FocusOut;
        RequestToggleFocus -= ToggleFocus;
    }

    private void FocusIn(GameObject target)
    {
        if (IsTweening || target == null) return;

        FocusableObject focusable = target.GetComponent<FocusableObject>();
        if (focusable == null)
        {
            focusable = target.AddComponent<FocusableObject>();
        }

        if (focusable.IsFocused) return;

        if (currentFocusedTarget != null && currentFocusedTarget != focusable)
        {
            FocusOut();
        }

        currentFocusedTarget = focusable;
        focusable.IsFocused = true;
        IsTweening = true;

        RectTransform rectTransform = target.GetComponent<RectTransform>();
        Sequence seq = DOTween.Sequence();

        if (rectTransform != null) // UI
        {
            rectTransform.SetAsLastSibling();
            seq.Join(rectTransform.DOAnchorPos(Vector2.zero, duration));
            seq.Join(rectTransform.DOScale(focusable.OriginalScale * targetScaleMultiplier, duration));
        }
        else // 3D 월드 오브젝트
        {
            if (mainCamera == null) mainCamera = Camera.main;
            Vector3 targetWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, distanceFromCamera));
            seq.Join(target.transform.DOMove(targetWorldPos, duration));
            seq.Join(target.transform.DOScale(focusable.OriginalScale * targetScaleMultiplier, duration));
            seq.Join(target.transform.DORotateQuaternion(Quaternion.LookRotation(mainCamera.transform.forward), duration));
        }

        seq.SetEase(Ease.OutCubic)
           .OnComplete(() => IsTweening = false);
    }

    private void FocusOut()
    {
        if (IsTweening || currentFocusedTarget == null) return;

        IsTweening = true;
        FocusableObject focusable = currentFocusedTarget;
        GameObject target = focusable.gameObject;
        RectTransform rectTransform = target.GetComponent<RectTransform>();

        Sequence seq = DOTween.Sequence();

        if (rectTransform != null)
        {
            seq.Join(rectTransform.DOAnchorPos(focusable.OriginalPosition, duration));
            seq.Join(rectTransform.DOScale(focusable.OriginalScale, duration));
            seq.SetEase(Ease.OutCubic)
               .OnComplete(() =>
               {
                   rectTransform.SetSiblingIndex(focusable.OriginalSiblingIndex);
                   focusable.IsFocused = false;
                   currentFocusedTarget = null;
                   IsTweening = false;
               });
        }
        else
        {
            seq.Join(target.transform.DOMove(focusable.OriginalPosition, duration));
            seq.Join(target.transform.DOScale(focusable.OriginalScale, duration));
            seq.Join(target.transform.DORotateQuaternion(focusable.OriginalRotation, duration));
            seq.SetEase(Ease.OutCubic)
               .OnComplete(() =>
               {
                   focusable.IsFocused = false;
                   currentFocusedTarget = null;
                   IsTweening = false;
               });
        }
    }

    private void ToggleFocus(GameObject target)
    {
        FocusableObject focusable = target.GetComponent<FocusableObject>();
        if (focusable != null && focusable.IsFocused)
        {
            FocusOut();
        }
        else
        {
            FocusIn(target);
        }
    }
}