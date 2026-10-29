using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Notion : MonoBehaviour
{
    public static Notion Instance { get; private set; }

    public TextMeshProUGUI NotionText;
    public Image NotionImage;

    [Header("알림 설정")]
    public float fadeDuration = 1.5f;
    public float displayDuration = 2f;

    private Sequence fadeSequence;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 초기 상태에서 이미지와 텍스트를 비활성화
        NotionImage.gameObject.SetActive(false);
        NotionText.gameObject.SetActive(false);
    }

    public void NotionUp(string text)
    {
        // 기존 애니메이션이 있다면 종료
        fadeSequence?.Kill();

        NotionText.text = text;

        // 이미지와 텍스트 활성화
        NotionImage.gameObject.SetActive(true);
        NotionText.gameObject.SetActive(true);

        // 투명도 초기화
        SetAlpha(1f);

        // 표시 후 천천히 사라지게 함
        fadeSequence = DOTween.Sequence();

        fadeSequence
            .AppendInterval(displayDuration)
            .Append(NotionImage.DOFade(0f, fadeDuration))
            .Join(NotionText.DOFade(0f, fadeDuration))
            .OnComplete(() =>
            {
                NotionImage.gameObject.SetActive(false);
                NotionText.gameObject.SetActive(false);
            });
    }
    public void NotionUpLocalization(string text)
    {
        // 기존 애니메이션이 있다면 종료
        fadeSequence?.Kill();

        NotionText.text = LocalizationManager.GetLocalizedString(text);

        // 이미지와 텍스트 활성화
        NotionImage.gameObject.SetActive(true);
        NotionText.gameObject.SetActive(true);

        // 투명도 초기화
        SetAlpha(1f);

        // 표시 후 천천히 사라지게 함
        fadeSequence = DOTween.Sequence();

        fadeSequence
            .AppendInterval(displayDuration)
            .Append(NotionImage.DOFade(0f, fadeDuration))
            .Join(NotionText.DOFade(0f, fadeDuration))
            .OnComplete(() =>
            {
                NotionImage.gameObject.SetActive(false);
                NotionText.gameObject.SetActive(false);
            });
    }
    public void NotionUpLocalizationFormat(
    string localizationKey,
    params object[] args)
    {
        // 기존 애니메이션 종료
        fadeSequence?.Kill();

        // 인자를 함께 전달하여 Localization 처리
        NotionText.text =
            LocalizationManager.GetLocalizedString(
                localizationKey,
                args
            );

        // 이미지와 텍스트 활성화
        NotionImage.gameObject.SetActive(true);
        NotionText.gameObject.SetActive(true);

        // 투명도 초기화
        SetAlpha(1f);

        // 표시 후 천천히 사라지게 함
        fadeSequence = DOTween.Sequence();

        fadeSequence
            .AppendInterval(displayDuration)
            .Append(NotionImage.DOFade(0f, fadeDuration))
            .Join(NotionText.DOFade(0f, fadeDuration))
            .OnComplete(() =>
            {
                NotionImage.gameObject.SetActive(false);
                NotionText.gameObject.SetActive(false);
            });
    }
    private void SetAlpha(float alpha)
    {
        Color imageColor = NotionImage.color;
        imageColor.a = alpha;
        NotionImage.color = imageColor;

        Color textColor = NotionText.color;
        textColor.a = alpha;
        NotionText.color = textColor;
    }

    private void OnDestroy()
    {
        fadeSequence?.Kill();
    }
}