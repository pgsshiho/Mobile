using UnityEngine;

public class Fronter : UnitBase
{
    public SkillType selectedSkill;
    public bool isTaunting = false;

    // [에너지 전환]: 고유 패시브 - 피격 시 호출되도록 TakeDamage 오버라이드
    public override void TakeDamage(float damage, float accure, float poison, float poisondamage)
    {
        float actualDamage = damage; // 실제 입은 데미지 계산 로직 추가 가능
        base.TakeDamage(damage, accure, poison, poisondamage);

        // 받은 데미지의 10%를 에너지로 치환
        energy += actualDamage * 0.1f;
        Debug.Log($"에너지 전환: {actualDamage * 0.1f} 충전됨.");
    }

    public void AttackStart(GameObject targetObject)
    {
        UnitBase targetUnit = targetObject.GetComponent<UnitBase>();

        switch (selectedSkill)
        {
            case SkillType.Taunt:
                isTaunting = true; // BattleManager에서 타겟팅 확률 계산 시 참조
                break;

            case SkillType.DefenseStance:
                armor *= 1.5f; // 방어력 50% 상승
                break;

            case SkillType.ForceFix:
                // 다음 턴 스킵 로직 필요
                currentHp += maxHp * 0.3f;
                break;

            case SkillType.Charge:
                targetObject.GetComponent<ITakeDamage>()?.TakeDamage(damage * 1.2f, accuracy, 0f, 0f);
                if (Random.Range(0, 100) < 30f) this.TakeDamage(5f, 100f, 0f, 0f); // 자신도 데미지
                break;
        }
    }
}