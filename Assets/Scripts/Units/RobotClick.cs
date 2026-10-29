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
    public TextMeshProUGUI AttackUp;
    public TextMeshProUGUI AttackMaxUp;
    public TextMeshProUGUI DepenceUp;
    public TextMeshProUGUI DepenceMaxUp;
    public TextMeshProUGUI AccuraryUp;
    public TextMeshProUGUI AccuraryMaxUp;
    public TextMeshProUGUI MaxHPUp;
    public TextMeshProUGUI MaxHPMaxUp;

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
        Debug.Log($"[RobotClick] 터치됨: {Self?.Unitname ?? "Self가 null"}");
        if (BattleManager.instance != null)
        {
            // 1. 분수대 대상 선택
            if (BattleManager.instance.IsSelectingFountainTarget)
            {
                BattleManager.instance.TryHandleFountainTarget(Self);
                Debug.Log("분수대 선택");
                return;
            }

            // 2. 블랙마켓 대상 선택
            if (BattleManager.instance.IsSelectingBlackMaket)
            {
                BattleManager.instance.TryHandleBlackMarketTarget(Self);
                Debug.Log("블랙마켓 선택");
                return;
            }
            //3. 수리점 대상 선택
            if (BattleManager.instance.IsSelectingFixerTarget)
            {
                BattleManager.instance.TryHandleFixerTarget(Self);
                Debug.Log("수리점 선택");
                return;
            }
            if (BattleManager.instance.IsSelectingBlackSmith)
            {
                BattleManager.instance.TryHandleBlackSmithTarget(Self);
                Debug.Log("수리점 선택");
                return;
            }
            // 3. 다른 대상 선택 중에는 일반 스탯 UI를 열지 않음
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
        criticalChance.text = Self.criticalChance.ToString();
        accuracy.text = Self.accuracy.ToString();
        dodgeChance.text = Self.dodgeChance.ToString();
        speed.text = Self.speed.ToString();
        energyMax.text = Self.energyMax.ToString();
        energyCurrent.text = Self.energyCurrent.ToString();
        AttackUp.text = Self.attackLevel.ToString();
        AttackMaxUp.text = Self.maxAttackLevel.ToString();
        DepenceUp.text = Self.defenseLevel.ToString();
        DepenceMaxUp.text = Self.maxDefenseLevel.ToString();
        AccuraryUp.text = Self.AccurancyLevel.ToString();
        AccuraryMaxUp.text = Self.maxAccurancyLevel.ToString();
        MaxHPUp.text = Self.MaxHPLevel.ToString();
        MaxHPMaxUp.text = Self.maxMaxHPLevel.ToString();

        if (Hp != null)
        {
            Hp.minValue = 0;
            Hp.maxValue = Self.maxHealth;
            Hp.value = Mathf.Clamp(Self.health, 0, Self.maxHealth);
            Hp.interactable = false;
        }
    }
}