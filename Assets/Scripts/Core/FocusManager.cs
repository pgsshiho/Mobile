using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class FocusManager : MonoBehaviour, IPointerClickHandler
{
    public static FocusManager Instance { get; private set; }

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
    public static Action RequestForceReset;
    public static Action<GameObject> RequestToggleFocus;

    private Camera mainCamera;
    private FocusableObject currentFocusedTarget;
    private Sequence currentSequence;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        mainCamera = Camera.main;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // 오브젝트가 활성화될 때 이벤트 연결
    private void OnEnable()
    {
        RequestFocusIn += FocusIn;
        RequestFocusOut += FocusOut;
        RequestForceReset += ForceReset;
        RequestToggleFocus += ToggleFocus;
    }

    // 오브젝트가 비활성화될 때 이벤트 해제 (메모리 누수 방지)
    private void OnDisable()
    {
        RequestFocusIn -= FocusIn;
        RequestFocusOut -= FocusOut;
        RequestForceReset -= ForceReset;
        RequestToggleFocus -= ToggleFocus;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject clickedObject = eventData.pointerPress;
        if (clickedObject != null)
        {
            ToggleFocus(clickedObject);
        }
    }

    private void FocusIn(GameObject target)
    {
        if (target == null) return;

        FocusableObject focusable = target.GetComponent<FocusableObject>();
        if (focusable == null)
        {
            focusable = target.AddComponent<FocusableObject>();
        }

        if (focusable.IsFocused) return;

        if (currentFocusedTarget != null && currentFocusedTarget != focusable)
        {
            ForceReset();
        }

        currentSequence?.Kill(false);
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
           .OnComplete(() =>
           {
               IsTweening = false;
               currentSequence = null;
           })
           .OnKill(() =>
           {
               IsTweening = false;
               currentSequence = null;
           });

        currentSequence = seq;
    }

    public void FocusOut()
    {
        if (currentFocusedTarget == null)
        {
            IsTweening = false;
            return;
        }

        currentSequence?.Kill(false);
        IsTweening = true;

        FocusableObject focusable = currentFocusedTarget;
        GameObject target = focusable.gameObject;

        if (!target.activeInHierarchy)
        {
            // 대상이 이미 비활성화된 경우 DOTween을 재생하지 않고 즉시 원복
            ForceReset();
            return;
        }

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
                   if (currentFocusedTarget == focusable)
                       currentFocusedTarget = null;
                   IsTweening = false;
                   currentSequence = null;
               })
               .OnKill(() =>
               {
                   IsTweening = false;
                   currentSequence = null;
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
                   if (currentFocusedTarget == focusable)
                       currentFocusedTarget = null;
                   IsTweening = false;
                   currentSequence = null;
               })
               .OnKill(() =>
               {
                   IsTweening = false;
                   currentSequence = null;
               });
        }

        currentSequence = seq;
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

    /// <summary>
    /// 전투 종료 등 즉각 포커스 해제가 필요한 경우 DOTween 없이 강제 초기화.
    /// 대상 오브젝트가 비활성화되기 전에 호출해야 함.
    /// </summary>
    public void ForceReset()
    {
        currentSequence?.Kill(false);
        currentSequence = null;

        if (currentFocusedTarget != null)
        {
            GameObject target = currentFocusedTarget.gameObject;
            if (target != null)
            {
                DOTween.Kill(target.transform, false);

                RectTransform rectTransform = target.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    DOTween.Kill(rectTransform, false);
                    rectTransform.anchoredPosition = currentFocusedTarget.OriginalPosition;
                    rectTransform.localScale = currentFocusedTarget.OriginalScale;
                    rectTransform.SetSiblingIndex(currentFocusedTarget.OriginalSiblingIndex);
                }
                else
                {
                    target.transform.position = currentFocusedTarget.OriginalPosition;
                    target.transform.localScale = currentFocusedTarget.OriginalScale;
                    target.transform.rotation = currentFocusedTarget.OriginalRotation;
                }
            }

            currentFocusedTarget.IsFocused = false;
            currentFocusedTarget = null;
        }

        IsTweening = false;
    }
}
