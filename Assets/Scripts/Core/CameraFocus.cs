
using DG.Tweening;
using UnityEngine;

public class CameraFocus : MonoBehaviour
{
    public static CameraFocus Instance { get; private set; }

    [Header("카메라 설정")]
    [SerializeField] private Camera targetCamera;

    [Header("포커스 연출")]
    [Tooltip("카메라가 이동하는 시간")]
    [SerializeField] private float duration = 1.0f;

    [Tooltip("버튼 위치를 기준으로 계산할 월드 공간 거리")]
    [Min(0.1f)]
    [SerializeField] private float distance = 3.0f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isFocused;
    private bool isTweening;

    private Tween currentTween;

    public bool IsFocused => isFocused;
    public bool IsTweening => isTweening;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    /// <summary>
    /// Screen Space - Overlay 버튼의 화면 위치를 기준으로 카메라를 이동시킵니다.
    /// </summary>
    public bool FocusOnButton(Transform buttonTransform)
    {
        if (buttonTransform == null || targetCamera == null)
            return false;

        if (isTweening || isFocused)
            return false;

        // Overlay UI의 Transform 좌표를 화면 좌표로 변환합니다.
        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                null,
                buttonTransform.position
            );

        // 화면 좌표에서 카메라가 바라보는 광선을 생성합니다.
        Ray ray = targetCamera.ScreenPointToRay(screenPosition);

        // 버튼 화면 위치에 대응하는 가상의 월드 좌표를 구합니다.
        Vector3 focusPoint = ray.GetPoint(distance);

        // 카메라의 기존 전방 방향을 기준으로 목표 위치를 계산합니다.
        Vector3 focusPosition =
            focusPoint - targetCamera.transform.forward * distance;

        Vector3 direction = focusPoint - focusPosition;

        if (direction.sqrMagnitude < 0.001f)
            return false;

        originalPosition = targetCamera.transform.position;
        originalRotation = targetCamera.transform.rotation;

        Quaternion focusRotation =
            Quaternion.LookRotation(direction.normalized);

        isFocused = true;
        isTweening = true;

        currentTween?.Kill();

        Sequence sequence = DOTween.Sequence();

        sequence.Join(
            targetCamera.transform.DOMove(focusPosition, duration)
        );

        sequence.Join(
            targetCamera.transform.DORotateQuaternion(
                focusRotation,
                duration
            )
        );

        sequence.SetEase(Ease.OutCubic)
            .OnComplete(() =>
            {
                isTweening = false;
                currentTween = null;
            })
            .OnKill(() =>
            {
                isTweening = false;
                currentTween = null;
            });

        currentTween = sequence;
        return true;
    }

    /// <summary>
    /// 포커스 전 카메라 위치와 회전으로 복귀합니다.
    /// </summary>
    public void FocusOut()
    {
        if (!isFocused || isTweening || targetCamera == null)
            return;

        isTweening = true;

        Sequence sequence = DOTween.Sequence();

        sequence.Join(
            targetCamera.transform.DOMove(
                originalPosition,
                duration
            )
        );

        sequence.Join(
            targetCamera.transform.DORotateQuaternion(
                originalRotation,
                duration
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
                currentTween = null;
            });

        currentTween = sequence;
    }

    private void OnDestroy()
    {
        currentTween?.Kill();

        if (Instance == this)
            Instance = null;
    }
}
