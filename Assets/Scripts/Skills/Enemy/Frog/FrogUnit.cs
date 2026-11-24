using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개구리 전용 Enemy 클래스.
/// 슬롯이 가득 차지 않았을 때 알 낳기를 사용하며 소환을 관리합니다.
/// </summary>
public class FrogUnit : Enemy
{
    [Header("개구리 스킬 참조")]
    public SkillData layEggSkill;

    protected override SkillData ChooseSkill()
    {
        // 적 슬롯이 꽉 찼으면 알 낳기 스킬 제외
        Enemy[] existingEnemies = Object.FindObjectsByType<Enemy>();
        int aliveCount = 0;
        foreach (Enemy e in existingEnemies)
        {
            if (e != null && e.health > 0) aliveCount++;
        }

        int maxSlots = (BattleManager.instance != null && BattleManager.instance.Enemy != null && BattleManager.instance.Enemy.Length > 0)
            ? BattleManager.instance.Enemy.Length
            : 4;

        if (aliveCount >= maxSlots && layEggSkill != null)
        {
            // 알 낳기 제외한 다른 스킬들 중 선택
            List<SkillData> validSkills = new List<SkillData>();
            foreach (SkillData s in skills)
            {
                if (s != null && s != layEggSkill) validSkills.Add(s);
            }

            if (validSkills.Count > 0)
            {
                return validSkills[Random.Range(0, validSkills.Count)];
            }
        }

        return base.ChooseSkill();
    }
}
