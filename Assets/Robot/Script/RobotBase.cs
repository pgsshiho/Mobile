using UnityEngine;

public class RobotBase : UnitBase
{
    [Header("Robot Exclusive")]
    public int currentEnergy;
    public int maxEnergy = 50;
    public bool isrockdown = false;
    public int neverdie = 10;

    public override void InitializeUnit()
    {
        base.InitializeUnit();
        currentEnergy = maxEnergy;
        isrockdown = false;
    }

    public override void TakeDamage(int damage, int accuracy, int ignoreDefense, int EnergyDamage)
    {
        base.TakeDamage(damage, accuracy, ignoreDefense, EnergyDamage);

        // 로봇 전용 로직: 전력 감소 및 고장 판정
        currentEnergy -= EnergyDamage;

        if (currentEnergy < 0 && !isrockdown)
            isrockdown = true;
        else if (currentEnergy > 0 && isrockdown && Random.Range(1, 3) == 1)
        {
            // 고장에서 회복할 확률 (50%)
        }
    }
    protected override void OnUnitDestroyed()
    {
        // 로봇 전용: 죽기 전에 'neverdie' 확률 체크
        int percent = Random.Range(1, 101);
        if (percent <= neverdie)
        {
            Debug.Log($"<color=cyan>경고! {gameObject.name}의 백업 회로가 작동하여 파괴를 면했습니다!</color>");
            currentHP = 1; // 기적적으로 체력 1로 생존
            return;
        }

        // 확률 체크 실패 시 부모의 파괴 로직 수행
        base.OnUnitDestroyed();
    }
}