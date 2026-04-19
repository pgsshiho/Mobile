using UnityEngine;


public class Cutter : UnitBase
{
    public SkillType selectedSkill;

    // 턴 시작 시 호출
    public void TurnStart()
    {
        if (isPoison) currentHp -= hitpoisondamage;
    }

    // 공격 실행 (전략 패턴 사용)
    public void ExecuteTurn()
    {
        GameObject target = targetingStrategy.SelectTarget(enemyList);
        if (target != null)
        {
            ITakeDamage targetInt = target.GetComponent<ITakeDamage>();
            switch (selectedSkill)
            {
                case SkillType.Cut: targetInt.TakeDamage(damage * 1.5f, accuracy, 0, 0); break;
                case SkillType.Overclock: damage *= 2.0f; break;
            }
        }
    }
    public void AttackStart(GameObject targetObject)
    {
        ITakeDamage target = targetObject.GetComponent<ITakeDamage>();
        if (target == null) return;

        switch (selectedSkill)
        {
            case SkillType.Cut:
                // [절단]: 단일 대상 큰 데미지 (1.5배)
                target.TakeDamage(damage * 1.5f, accuracy, 0f, 0f);
                break;

            case SkillType.Divide:
                // [가르기]: 전열 공격 (1.2배)
                target.TakeDamage(damage * 1.2f, accuracy, 0f, 0f);
                Debug.Log("전열을 가르며 공격합니다!");
                break;

            case SkillType.Overclock:
                // [폭주 모드]: 공격력 2배 영구 증가 (이 로직은 턴 종료 시 혹은 해제 로직이 필요할 수 있음)
                damage *= 2.0f;
                Debug.Log("폭주 모드! 공격력이 2배로 증가합니다.");
                break;

            case SkillType.Friction:
                // [마찰]: 고열 발생 (데미지 0.8배 + 녹/상태이상 5만큼 부여)
                target.TakeDamage(damage * 0.8f, accuracy, 1f, 5f);
                Debug.Log("마찰 공격! 상대에게 고열(지속 데미지)을 부여했습니다.");
                break;

            case SkillType.Ready:
                // [제거 준비]: 자가 버프 (속도 +10, 크리티컬 확률 +20%)
                speed += 10f;
                criticalChance += 20f;
                Debug.Log("제거 준비 완료! 속도와 크리티컬이 상승합니다.");
                break;

            case SkillType.PointAttack:
                // [약점 타격]: 상태이상 대상에게 1.5배 데미지
                float multiplier = 1.0f;
                // 타겟이 UnitBase를 상속받았다면 상태이상 확인
                if (targetObject.TryGetComponent<UnitBase>(out var unit) && unit.isPoison)
                {
                    multiplier = 1.5f;
                    Debug.Log("약점 타격 적중! 추가 피해를 줍니다.");
                }
                target.TakeDamage(damage * multiplier, accuracy, 0f, 0f);
                break;
        }
    }
}