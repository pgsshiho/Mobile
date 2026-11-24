using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Eagle/LockOn")]
public class LockOn : SkillBase
{
    [Header("Mark Settings & Duration")]
    [Range(0, 100)]
    [Tooltip("표식 부여 확률 (기획: 100%, 대상의 specialArmor/special ammo에 의해 감쇄)")]
    public int markChance = 100;

    [Tooltip("표식 지속 턴 (기획: 3턴)")]
    public int markDuration = 3;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (target == null) return;

        // 공식: 상태이상 확률 = 유닛(스킬) 확률 - special ammo(specialArmor)
        int finalChance = Mathf.Clamp(markChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            target.AddMark(markDuration);
            Debug.Log($"{target.Unitname}에게 조준 고정 표식 부여 완료 ({markDuration}턴 지속, 아군 공격 시 명중률 100%, 성공률: {finalChance}%)!");
        }
        else
        {
            Debug.Log($"{target.Unitname} 표식 저항 성공! (확률 {markChance}% - 특수장갑 {target.specialArmor}% = {finalChance}%)");
        }
    }
}
