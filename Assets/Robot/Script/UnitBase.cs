using UnityEngine;

public abstract class UnitBase : MonoBehaviour, ITakeDamage
{
    [Header("Base Stats")]
    public int currentHP;
    public int maxHP = 100;
    public int Avoid = 10;
    public int spd = 10;
    protected virtual void Awake() => InitializeUnit();

    public virtual void InitializeUnit()
    {
        currentHP = maxHP;
    }

    public virtual void TakeDamage(int damage, int accuracy, int ignoreDefense, int EnergyDamage)
    {
        int accuracys = Random.Range(0, 100);
        int realaccuracy = accuracy - Avoid;

        if (realaccuracy > accuracys)
        {
            Debug.Log($"<color=yellow>{gameObject.name}</color>이(가) 공격을 회피했습니다!");
            return;
        }

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        Debug.Log($"<color=red>{gameObject.name}</color> 피격! 남은 체력: {currentHP}/{maxHP}");

        if (currentHP <= 0) OnUnitDestroyed();
    }

    protected virtual void OnUnitDestroyed()
    {
        // 로봇/적 공통 파괴 처리 (애니메이션, 데이터 삭제 등)
        Debug.Log($"{gameObject.name}이(가) 파괴되었습니다.");
        Destroy(gameObject, 2f);
    }
}