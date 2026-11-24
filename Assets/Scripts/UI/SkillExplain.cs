
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillExplain : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Skill Data")]
    public SkillData myskill;

    [Header("Long Press Settings")]
    [Tooltip("모바일 터치 환경에 맞춰 롱프레스 시간을 0.3초로 단축")]
    [SerializeField] private float durationThreshold = 0.3f;

    [Tooltip("손을 떼었을 때 설명창을 자동으로 닫을지 여부 (기본: false)")]
    [SerializeField] private bool closeOnPointerUp = false;

    private bool isPointerDown = false;
    private float pointerDownTime = 0f;
    private bool hasTriggeredThisPress = false;

    [Header("UI References")]
    public GameObject skillExplainUI;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescriptionText;

    private void Start()
    {
        SetSkill();

        if (skillExplainUI != null)
            skillExplainUI.SetActive(false);
    }

    public void SetSkill()
    {
        if (myskill == null)
        {
            Debug.LogWarning($"[{gameObject.name}] SkillData가 연결되지 않았습니다.", this);
            return;
        }

        if (skillNameText != null)
            skillNameText.text = myskill.skillName;

        if (skillDescriptionText != null)
            skillDescriptionText.text = myskill.description;
    }

    private void Update()
    {
        if (!isPointerDown || hasTriggeredThisPress)
            return;

        if (Time.time - pointerDownTime >= durationThreshold)
        {
            hasTriggeredThisPress = true;

            if (skillExplainUI != null)
            {
                SetSkill();
                skillExplainUI.SetActive(true);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        hasTriggeredThisPress = false;
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;

        if (closeOnPointerUp && skillExplainUI != null && skillExplainUI.activeSelf)
        {
            skillExplainUI.SetActive(false);
        }
    }

    /// <summary>모바일에서 설명창을 탭하여 직접 닫을 수 있는 편의 함수</summary>
    public void CloseExplainUI()
    {
        if (skillExplainUI != null)
            skillExplainUI.SetActive(false);
    }
}
