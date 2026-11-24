using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Laptor/Overcharge")]
public class Overcharge : SkillBase
{
    public BuffData overchargeBuff;

    [Header("Overheat Settings")]
    [Range(0, 100)]
    [Tooltip("과열 발생 확률 (자신의 specialArmor/special ammo에 의해 감쇄)")]
    public int overheatChance = 30;

    [Tooltip("과열 지속 턴 (기획: 2턴)")]
    public int overheatDuration = 2;

    [Tooltip("과열 턴당 피해량")]
    public int overheatDamage = 4;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        if (user == null) return;

        if (overchargeBuff != null)
        {
            user.AddBuff(overchargeBuff);
        }

        // 공식: 상태이상 확률 = 유닛 확률 - special ammo(specialArmor)
        int finalChance = Mathf.Clamp(overheatChance - user.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            int appliedDamage = overheatDamage > 0 ? overheatDamage : user.overheatInflictDamage;
            user.AddStatus(StatusType.Overheat, overheatDuration, appliedDamage);
            Debug.Log($"{user.Unitname} 폭주 모드로 인한 과열 발생! ({overheatDuration}턴 지속, 턴당 {appliedDamage}피해, 확률: {finalChance}%)");
        }
    }
}
