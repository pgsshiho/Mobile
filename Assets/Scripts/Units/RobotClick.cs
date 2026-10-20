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
        // 아이템 대상 선택 중이면 스탯창을 열지 않는다.
        if (BattleManager.instance != null &&
            BattleManager.instance.IsSelectingItemTarget)
        {
            return;
        }

        TextSet();
        myUI.SetActive(true);
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