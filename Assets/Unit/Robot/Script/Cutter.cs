using UnityEngine;

public enum SkillType { Cut, Divide, Overclock, Friction, Ready, PointAttack }

public class Cutter : UnitBase
{
    public SkillType selectedSkill;

    public void TurnStart()
    {
        if (isPoison)
        {
            currentHp -= hitpoisondamage;
            Debug.Log($"녹(상태이상)으로 인해 {hitpoisondamage}의 피해를 입었습니다.");
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
                // [가르기]: 전열 공격, 이동 로직은 별도 처리 필요
                target.TakeDamage(damage * 1.2f, accuracy, 0f, 0f);
                Debug.Log("전열로 이동하며 가르기 공격!");
                break;

            case SkillType.Overclock:
                // [폭주 모드]: 공격력 2배
                damage *= 2.0f;
                Debug.Log("폭주 모드! 공격력이 2배로 증가합니다.");
                break;

            case SkillType.Friction:
                // [마찰]: 고열(상태이상) 발생
                // 마지막 인자(poisondamage)에 지속 데미지를 전달하여 상태이상 부여
                target.TakeDamage(damage * 0.8f, accuracy, 1f, 5f);
                Debug.Log("마찰 공격! 상대에게 고열을 발생시켰습니다.");
                break;

            case SkillType.Ready:
                // [제거 준비]: 버프 (속도, 크리티컬 상승)
                speed += 10f;
                criticalChance += 20f;
                Debug.Log("제거 준비 완료! 속도와 치명타 확률이 상승합니다.");
                break;

            case SkillType.PointAttack:
                // [약점 타격]: 상태이상 있을 시 1.5배 피해
                // 상대방이 ITakeDamage를 상속받은 객체이므로 
                // 해당 객체에서 상태 이상 확인 로직이 필요합니다.
                float multiplier = 1.0f;

                // 타겟의 상태이상 여부를 확인하는 방식 (필요시 ITakeDamage에 메서드 추가)
                if (target is UnitBase unit && unit.isPoison)
                {
                    multiplier = 1.5f;
                    Debug.Log("약점 타격! 상태이상 대상에게 추가 피해.");
                }
                target.TakeDamage(damage * multiplier, accuracy, 0f, 0f);
                break;

            default:
                Debug.Log("준비되지 않은 스킬입니다.");
                break;
        }
    }
}