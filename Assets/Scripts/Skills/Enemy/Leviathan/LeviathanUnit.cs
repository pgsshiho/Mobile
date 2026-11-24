using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레비아탄 (최상위 외계 생명체 / 챕터 보스) 전용 Enemy 클래스.
/// 시작 시 합선 / 오염 상태를 보유합니다.
/// 체력이 100 이하가 되면 [진화] 상태가 되어 포식 본능 및 절대 포식 스킬을 해금합니다.
/// </summary>
public class LeviathanUnit : Enemy
{
    [Header("레비아탄 진화 기믹")]
    [Tooltip("진화 발동 체력 기준 (기획: 100 이하)")]
    public int evolutionHpThreshold = 100;
    public bool isEvolved = false;

    [Tooltip("진화 후 해금되는 스킬: 포식 본능")]
    public SkillData predatoryInstinctSkill;

    [Tooltip("진화 후 해금되는 스킬: 절대 포식")]
    public SkillData absolutePredationSkill;

    [Header("절대 포식 충전 기믹")]
    [Tooltip("절대 포식 준비 턴 여부")]
    public bool isChargingAbsolutePredation = false;

    protected override void Start()
    {
        base.Start();

        // 초기 상태이상: 합선 및 오염
        AddStatus(StatusType.ShortCircuit, 3);
        AddStatus(StatusType.WeaponPollution, 3);
    }

    public override void MyTurn()
    {
        // 체력 100 이하 체크 -> 진화 해금
        if (!isEvolved && health <= evolutionHpThreshold && health > 0)
        {
            TriggerEvolution();
        }

        base.MyTurn();
    }

    private void TriggerEvolution()
    {
        isEvolved = true;
        Debug.Log($"<color=purple><b>[레비아탄 궁극 진화!]</b></color> {Unitname}의 체력이 {health} 이하로 떨어져 진화했습니다! 최상위 포식 스킬이 해금됩니다!");

        // AI 타겟 모드를 약자 우선으로 전환
        targetMode = EnemyTargetMode.LowestHp;

        if (predatoryInstinctSkill != null && !skills.Contains(predatoryInstinctSkill))
        {
            skills.Add(predatoryInstinctSkill);
        }
        if (absolutePredationSkill != null && !skills.Contains(absolutePredationSkill))
        {
            skills.Add(absolutePredationSkill);
        }
    }

    protected override SkillData ChooseSkill()
    {
        if (isEvolved)
        {
            // 절대 포식 준비 완료 상태라면 이번 턴에 절대 포식 확정 시전
            if (isChargingAbsolutePredation && absolutePredationSkill != null)
            {
                isChargingAbsolutePredation = false;
                Debug.Log($"<color=red><b>[절대 포식 전개]</b></color> {Unitname}이(가) 준비를 마치고 모든 대상을 집어삼킵니다!");
                return absolutePredationSkill;
            }

            // 진화 후: 절대 포식 시전 준비 확률 (35%)
            if (!isChargingAbsolutePredation && absolutePredationSkill != null && Random.value < 0.35f)
            {
                isChargingAbsolutePredation = true;
                Debug.Log($"<color=red><b>[경고: 절대 포식 충전 시작!]</b></color> {Unitname}의 거대한 입이 차원을 왜곡하며 다음 턴 절대 포식을 준비합니다!");
                return null;
            }

            // 포식 본능 스킬 우선권 (60%)
            if (predatoryInstinctSkill != null && Random.value < 0.6f)
            {
                return predatoryInstinctSkill;
            }
        }

        return base.ChooseSkill();
    }
}
