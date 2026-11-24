using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 해군선장 전용 Enemy 클래스.
/// 대포 발사 후 반드시 다음 턴에 재장전을 수행하며, 지정된 표식 타겟을 관리합니다.
/// </summary>
public class NavyCaptainUnit : Enemy
{
    [Header("해군선장 고유 기믹")]
    [Tooltip("대포 발사 후 재장전 필요 상태 여부")]
    public bool needsReload = false;

    [Tooltip("재장전 스킬 데이터 (대포 발사 후 확정 선택)")]
    public SkillData reloadSkill;

    [Tooltip("대포 발사 스킬 데이터")]
    public SkillData cannonSkill;

    [Tooltip("현재 지정된 목표 플레이어 (소환된 부하들이 우선 타겟팅)")]
    public Unit designatedTarget;

    protected override SkillData ChooseSkill()
    {
        // 1. 대포 발사 후에는 무조건 재장전 스킬 확정 사용
        if (needsReload && reloadSkill != null)
        {
            return reloadSkill;
        }

        // 2. 재장전 상태가 아닐 때는 재장전 스킬을 제외하고 선택
        List<SkillData> available = new List<SkillData>();
        foreach (SkillData s in skills)
        {
            if (s != null && s != reloadSkill)
            {
                available.Add(s);
            }
        }

        if (available.Count > 0)
        {
            return available[Random.Range(0, available.Count)];
        }

        return base.ChooseSkill();
    }
}
