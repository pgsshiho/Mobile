using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RobotClick : MonoBehaviour, IPointerClickHandler
{
    public Unit Self;
    public GameObject myUI;

    [Header("각 스탯들")]
    public TextMeshProUGUI maxHealth;
    public TextMeshProUGUI health;
    public TextMeshProUGUI emergencyPower;
    public TextMeshProUGUI attackPower;
    public TextMeshProUGUI defensePower;
    public TextMeshProUGUI specialArmor;
    public TextMeshProUGUI criticalChance;
    public TextMeshProUGUI accuracy;
    public TextMeshProUGUI dodgeChance;
    public TextMeshProUGUI speed;
    public TextMeshProUGUI energyMax;
    public TextMeshProUGUI energyCurrent;

    public Slider Hp;

    private PlayerUnit pu;

    void Awake()
    {
        pu = GetComponent<PlayerUnit>();
    }

    void Start()
    {
        TextSet();
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        if (BattleManager.instance != null)
        {
            // 분수대 선택은 일반 대상 선택보다 먼저 처리한다.
            if (BattleManager.instance.IsSelectingFountainTarget)
            {
                BattleManager.instance.TryHandleFountainTarget(Self);
                return;
            }

            // 기존 아이템 / Fixer / 스킬 대상 선택 처리
            if (BattleManager.instance.IsSelectingAnyTarget ||
                BattleManager.instance.IsSelectingItemTarget ||
                BattleManager.instance.IsSelectingFixerTarget)
            {
                return;
            }
        }

        TextSet();

        if (myUI != null)
            myUI.SetActive(true);
    }



    private void OnDisable()
    {
        // 유닛이 필드에서 회수(비활성화)될 때 열려 있던 스탯창도 함께 닫아준다.
        if (myUI != null && myUI.activeSelf)
        {
            myUI.SetActive(false);
        }
    }

    public void TextSet()
    {
        if (Self == null)
            return;

        maxHealth.text = Self.maxHealth.ToString();
        health.text = Self.health.ToString();
        emergencyPower.text = Self.emergencyPower.ToString();
        attackPower.text = Self.attackPower.ToString();
        defensePower.text = Self.defensePower.ToString();
        specialArmor.text = Self.specialArmor.ToString();
        accuracy.text = Self.accuracy.ToString();
        dodgeChance.text = Self.dodgeChance.ToString();
        speed.text = Self.speed.ToString();
        energyMax.text = Self.energyMax.ToString();
        energyCurrent.text = Self.energyCurrent.ToString();

        if (Hp != null)
        {
            Hp.minValue = 0;
            Hp.maxValue = Self.maxHealth;
            Hp.value = Mathf.Clamp(Self.health, 0, Self.maxHealth);
            Hp.interactable = false;
        }
    }
}