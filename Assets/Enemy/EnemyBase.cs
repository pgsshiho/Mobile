using UnityEngine;

public class EnemyBase : MonoBehaviour, ITakeDamage
{
    // [상태 값]
    [Header("Current Status")]
    public int currentHP;
    public int currentEnergy;
    public int currentHeat;   // 과열도 (0~100)

    // [기본 능력치]
    [Header("Base Stats")]
    public int maxHP = 100;
    public int attack = 10;
    public int maxEnergy = 50;
    public int Chritial = 10;
    public int Avoid = 10; // 회피 확률

    // [수리 및 관리]
    [Header("Maintenance")]
    public int currentFixCount;
    public int maxFixCount = 3;
    public bool isrockdown = false;
    void Awake()
    {
        InitializeRobot();
    }

    // 초기화 함수 (필요시 외부에서도 호출 가능)
    public void InitializeRobot()
    {
        currentHP = maxHP;
        currentEnergy = maxEnergy;
        currentHeat = 0;
        currentFixCount = maxFixCount;
        isrockdown = false;
    }
    public void TakeDamage(int damage, int accuracy, int ignoreDefense, int EnergyDamage)
    {
        int accuracys = Random.Range(0, 100);
        int realaccuracy = accuracy - Avoid;
        if (realaccuracy > accuracys)
        {
            Debug.Log($"<color=yellow>{gameObject.name}</color>이(가) 공격을 회피했습니다! (회피 확률: {realaccuracy}%, 랜덤 값: {accuracys})");
            return;
        }
        int finalDamage = damage;

        currentHP -= finalDamage;
        currentEnergy -= EnergyDamage;
        if (currentHP < 0) currentHP = 0;
        if (currentEnergy < 0 && isrockdown == false) energydown();
        else if (currentEnergy > 0 && isrockdown)
        {
            int g = Random.Range(1, 3);
            if (g == 1)
            {
                OnRobotDestroyed();
            }
        }
        Debug.Log($"<color=red>{gameObject.name}</color>이(가) {finalDamage}의 데미지를 입었습니다. 남은 체력: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            OnRobotDestroyed();
        }
    }
    private void OnRobotDestroyed()
    {
        Debug.Log($"{gameObject.name}이(가) 완전히 파괴되었습니다. 회수 불가능 상태.");
        Destroy(gameObject, 2f);
    }
    private void energydown()
    {
        int ran = Random.Range(1, 4);
    }
}