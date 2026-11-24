using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 익사체 전용 Enemy 클래스.
/// 물귀신 준비 후 다음 턴 확정 물귀신(즉사)을 연계합니다.
/// </summary>
public class DrownedUnit : Enemy
{
    [Header("익사체 특수 기믹")]
    [Tooltip("물귀신 준비 완료 여부")]
    public bool isWaterGhostReady = false;

    [Tooltip("물귀신 즉사 스킬 (준비 완료 시 우선 시전)")]
    public SkillData waterGhostSkill;

    [Tooltip("물귀신 준비 스킬")]
    public SkillData readySkill;

    protected override SkillData ChooseSkill()
    {
        // 1. 물귀신이 준비되어 있다면 반드시 물귀신 즉사 스킬 시전
        if (isWaterGhostReady && waterGhostSkill != null)
        {
            return waterGhostSkill;
        }

        // 2. 동료 적들의 체력이 낮으면 술 잔치 고려 (Enemy 기본 ChooseSkill 활용 가능)
        return base.ChooseSkill();
    }
}
