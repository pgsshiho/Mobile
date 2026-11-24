using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Laptor/Friction")]
public class Friction : SkillBase
{
    [Header("Overheat Settings")]
    [Range(0, 100)]
    [Tooltip("상대에게 고열(과열) 발생 확률 (기획: 70%, 대상의 specialArmor/special ammo에 의해 감쇄)")]
    public int overheatChance = 70;

    [Tooltip("과열 지속 턴")]
    public int duration = 3;

    [Tooltip("과열 턴당 피해량")]
    public int dotDamage = 4;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 마찰이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        if (damage > 0)
        {
            target.TakeDamage(damage);
        }

        // 공식: 상태이상 확률 = 유닛(스킬) 확률 - special ammo(specialArmor)
        int finalChance = Mathf.Clamp(overheatChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            int appliedDamage = dotDamage > 0 ? dotDamage : (user != null ? user.overheatInflictDamage : 4);
            target.AddStatus(StatusType.Overheat, duration, appliedDamage);
            Debug.Log($"{target.Unitname} 마찰 발생! 과열({duration}턴, 턴당 {appliedDamage}피해, 확률: {finalChance}%) 부여 완료!");
        }
        else
        {
            Debug.Log($"{target.Unitname} 고열 저항 성공! (고열 확률 {overheatChance}% - 특수장갑 {target.specialArmor}% = {finalChance}%)");
        }
    }
}
