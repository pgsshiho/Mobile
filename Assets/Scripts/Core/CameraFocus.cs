
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFocus : MonoBehaviour
{
    public static CameraFocus Instance { get; private set; }

    [Header("카메라 설정")]
    [SerializeField] private Camera targetCamera;

    [Header("포커스 연출")]
    [Tooltip("카메라가 이동하는 시간")]
    [SerializeField] private float duration = 1.0f;

    [Tooltip("대상으로부터 떨어질 거리")]
    [SerializeField] private float distance = 3.0f;

    [Tooltip("대상보다 위쪽에서 바라볼 높이")]
    [SerializeField] private float heightOffset = 1.0f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isFocused;
    private bool isTweening;

    private Tween currentTween;

    public bool IsFocused => isFocused;
    public bool IsTweening => isTweening;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    public void FocusOnClickedButton()
    {
        GameObject clickedButton =
            EventSystem.current.currentSelectedGameObject;

        if (clickedButton == null)
        {
            Debug.LogWarning("선택된 버튼을 찾을 수 없습니다.");
            return;
        }

        FocusOn(clickedButton);
    }

    /// <summary>
    /// 지정한 오브젝트 쪽으로 카메라를 이동시킵니다.
    /// </summary>
    public void FocusOn(GameObject target)
    {
        if (target == null || targetCamera == null || isTweening)
            return;

        if (isFocused)
            return;

        // 기존 카메라 위치와 회전 저장
        originalPosition = targetCamera.transform.position;
        originalRotation = targetCamera.transform.rotation;

        Transform targetTransform = target.transform;

        // 대상의 앞쪽에서 바라보도록 위치 계산
        Vector3 focusPosition =
            targetTransform.position
            - targetTransform.forward * distance
            + Vector3.up * heightOffset;

        // 대상의 중심을 바라보도록 회전 계산
        Vector3 direction =
            targetTransform.position - focusPosition;

        Quaternion focusRotation =
            Quaternion.LookRotation(direction.normalized);

        isTweening = true;
        isFocused = true;

        currentTween?.Kill();

        Sequence sequence = DOTween.Sequence();

        sequence.Join(
            targetCamera.transform.DOMove(focusPosition, duration)
        );

        sequence.Join(
            targetCamera.transform.DORotateQuaternion(
                focusRotation, duration
            )
        );

        sequence.SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                isTweening = false;
            })
            .OnKill(() =>
            {
                isTweening = false;
            });

        currentTween = sequence;
    }

    /// <summary>
    /// 원래 카메라 위치로 돌아갑니다.
    /// </summary>
    public void FocusOut()
    {
        if (!isFocused || isTweening || targetCamera == null)
            return;

        isTweening = true;

        currentTween?.Kill();

        Sequence sequence = DOTween.Sequence();

        sequence.Join(
            targetCamera.transform.DOMove(
                originalPosition, duration
            )
        );

        sequence.Join(
            targetCamera.transform.DORotateQuaternion(
                originalRotation, duration
            )
        );

        sequence.SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                isFocused = false;
                isTweening = false;
                currentTween = null;
            })
            .OnKill(() =>
            {
                isTweening = false;
            });

        currentTween = sequence;
    }

    /// <summary>
    /// 포커스 상태에 따라 이동 또는 복귀합니다.
    /// </summary>
    public void ToggleFocus(GameObject target)
    {
        if (isFocused)
            FocusOut();
        else
            FocusOn(target);
    }

    private void OnDestroy()
    {
        currentTween?.Kill();
    }
}
