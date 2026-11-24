using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 늘어나기(분열) 시 생성되는 지네 마디 유닛.
/// 메인 지네와 정보를 공유하며 독립된 턴을 가집니다.
/// </summary>
public class CentipedeSegmentUnit : Enemy
{
    [Header("마디 소속 정보")]
    public CentipedeUnit mainBody;
    public int segmentIndex = 1;
    public float originalMaxHp = 210f;

    [Tooltip("협공 스킬 발동 시 이번 턴 소모 여부")]
    public bool hasActedInCoordinatedAttack = false;

    [Header("스킬 데이터 참조")]
    public SkillData stretchSkill;
    public SkillData coilSkill;
    public SkillData shrinkSkill;
    public SkillData poisonSpraySkill;
    public SkillData pincerSkill;
    public SkillData biteSkill;

    public override void MyTurn()
    {
        // 협공으로 인해 이미 턴을 소모한 경우 이번 턴 자동 스킵
        if (hasActedInCoordinatedAttack)
        {
            hasActedInCoordinatedAttack = false;
            Debug.Log($"<color=orange>[협공 턴 소모]</color> {Unitname}은(는) 협공 참여로 이번 턴 행동을 마쳤습니다.");
            if (TurnManager.instance != null) TurnManager.instance.EndTurn();
            return;
        }

        base.MyTurn();
    }

    protected override SkillData ChooseSkill()
    {
        // 분열 상태의 스킬 중 선택 (줄어들기, 독 뿌리기, 물기, 협공)
        List<SkillData> splitSkills = new List<SkillData>();

        if (shrinkSkill != null) splitSkills.Add(shrinkSkill);
        if (poisonSpraySkill != null) splitSkills.Add(poisonSpraySkill);
        if (biteSkill != null) splitSkills.Add(biteSkill);
        if (pincerSkill != null) splitSkills.Add(pincerSkill);

        if (splitSkills.Count > 0)
        {
            return splitSkills[Random.Range(0, splitSkills.Count)];
        }

        return base.ChooseSkill();
    }

    /// <summary>
    /// 메인 본체가 사망했을 때 이 마디가 새로운 메인 본체로 승격합니다.
    /// </summary>
    public void PromoteToMainBody(CentipedeUnit oldMain)
    {
        CentipedeUnit newMain = gameObject.AddComponent<CentipedeUnit>();
        newMain.originalMaxHp = originalMaxHp;
        newMain.isSplit = true;
        newMain.stretchSkill = stretchSkill;
        newMain.coilSkill = coilSkill;
        newMain.shrinkSkill = shrinkSkill;
        newMain.poisonSpraySkill = poisonSpraySkill;
        newMain.pincerSkill = pincerSkill;
        newMain.biteSkill = biteSkill;
        newMain.spawnedSegments = oldMain.spawnedSegments;

        // 다른 마디들의 mainBody 참조 갱신
        foreach (var seg in newMain.spawnedSegments)
        {
            if (seg != null) seg.mainBody = newMain;
        }

        Unitname = "지네 (본체)";
        Destroy(this);
        Debug.Log("<color=yellow>[지네 본체 교체]</color> 기존 본체 사망으로 생존 마디가 지네의 새로운 본체가 되었습니다!");
    }

    public override void Die()
    {
        if (BattleManager.instance != null)
        {
            BattleManager.instance.UnregisterEnemy(this);
        }
        if (TurnManager.instance != null)
        {
            TurnManager.instance.RemoveUnit(this);
        }
        if (mainBody != null && mainBody.spawnedSegments.Contains(this))
        {
            mainBody.spawnedSegments.Remove(this);
        }

        base.Die();
    }
}
