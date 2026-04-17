using UnityEngine;

public class Sniper : UnitBase
{
    public SkillType selectedSkill;

    public void AttackStart(GameObject targetObject)
    {
        ITakeDamage target = targetObject.GetComponent<ITakeDamage>();
        if (target == null) return;

        switch (selectedSkill)
        {
            case SkillType.WeakPointShot:
                // [약점 저격]: 치명타 확률 대폭 상승
                float critMultiplier = (Random.Range(0, 100) < (criticalChance + 30f)) ? 2.0f : 1.0f;
                target.TakeDamage(damage * critMultiplier, accuracy, 0f, 0f);
                break;

            case SkillType.AimShot:
                // [조준 사격]: 1턴 준비 후 다음 턴 2.5배 데미지
                Debug.Log("조준 중... 다음 턴에 강력한 공격!");
                // 턴 관리 로직에서 다음 공격 시 데미지 배율 적용
                break;

            case SkillType.FlareShot:
                // [신호 탄환]: 회피율 0으로 감소
                if (targetObject.TryGetComponent<UnitBase>(out var targetUnit))
                {
                    targetUnit.evasionRate = 0f;
                    Debug.Log("신호 탄환 명중! 대상의 회피율이 0이 되었습니다.");
                }
                break;

            case SkillType.DataCorruption:
                // [데이터 오염탄]: 스킬 사용 확률 감소 (고유 로직 필요)
                Debug.Log("데이터 오염! 적의 시스템이 불안정해집니다.");
                break;

            case SkillType.PiercingShot:
                // [관통 사격]: 전/후열 전체 공격
                // BattleManager를 통해 리스트의 모든 적에게 데미지 전달
                break;

            case SkillType.LockOn:
                // [조준 고정]: 3턴간 명중률 100% 버프
                Debug.Log("대상을 조준 고정했습니다. 아군 명중률 100%!");
                break;
        }
    }
}