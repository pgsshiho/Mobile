using UnityEngine;

public class Fixer : UnitBase
{
    public SkillType selectedSkill;

    public void AttackStart(GameObject targetObject)
    {
        ITakeDamage targetInterface = targetObject.GetComponent<ITakeDamage>();
        UnitBase targetUnit = targetObject.GetComponent<UnitBase>();

        switch (selectedSkill)
        {
            case SkillType.EmergencyWelding:
                // [긴급 용접]: 체력 회복 (데미지에 마이너스 값을 넣어 회복으로 처리하거나 별도 함수 사용)
                if (targetUnit != null) targetUnit.currentHp = Mathf.Min(targetUnit.currentHp + 20f, targetUnit.maxHp);
                break;

            case SkillType.WrenchThrow:
                // [렌치 던지기]: 확률적 기절 (기절 로직은 TurnManager에서 체크할 bool 변수 필요)
                targetInterface?.TakeDamage(damage * 0.5f, accuracy, 0f, 0f);
                if (Random.Range(0, 100) < 30f) Debug.Log("상대 기절!");
                break;

            case SkillType.Recycle:
                // [재활용]: 로직 구현을 위해 대상 2명이 필요함 (여기선 타겟의 체력을 깎고 자신을 회복하는 예시)
                targetUnit.currentHp -= 15f;
                this.currentHp += 15f;
                break;

            case SkillType.WrenchSwing:
                // 1~2열 조건은 BattleManager에서 타겟팅 제한으로 구현
                targetInterface?.TakeDamage(damage * 1.0f, accuracy, 0f, 0f);
                break;
        }
    }
}