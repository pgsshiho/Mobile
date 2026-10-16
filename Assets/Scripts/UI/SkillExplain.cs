
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillExplain : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Skill Data")]
    public SkillData myskill;

    [Header("Long Press Settings")]
    [SerializeField] private float durationThreshold = 0.5f;

    private bool isPointerDown = false;
    private float pointerDownTime = 0f;

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
        if (!isPointerDown)
            return;

        if (Time.time - pointerDownTime >= durationThreshold)
        {
            isPointerDown = false;

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
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
    }
}
