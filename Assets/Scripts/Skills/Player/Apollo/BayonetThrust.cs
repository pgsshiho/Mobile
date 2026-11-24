using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Apollo/BayonetThrust")]
public class BayonetThrust : SkillBase
{
    [Header("Bleed Settings")]
    [Range(0, 100)]
    [Tooltip("출혈 부여 확률 (기획: 100%, 대상의 specialArmor/special ammo에 의해 감쇄)")]
    public int bleedChance = 100;

    [Tooltip("출혈 지속 턴")]
    public int bleedDuration = 3;

    [Tooltip("출혈 턴당 방어무시 피해량")]
    public int bleedDamage = 5;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 총검술이 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        // 공식: 상태이상 확률 = 유닛(스킬) 확률 - special ammo(specialArmor)
        int finalChance = Mathf.Clamp(bleedChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            int appliedDamage = bleedDamage > 0 ? bleedDamage : user.bleedInflictDamage;
            target.AddStatus(StatusType.Bleeding, bleedDuration, appliedDamage);
            Debug.Log($"{user.Unitname} 총검 찌르기 적중! {target.Unitname}에게 {damage} 피해 및 출혈({bleedDuration}턴, 턴당 {appliedDamage}피해, 확률: {finalChance}%) 부여!");
        }
        else
        {
            Debug.Log($"{target.Unitname} 총검 출혈 저항 성공! (출혈 확률 {bleedChance}% - 특수장갑 {target.specialArmor}% = {finalChance}%)");
        }
    }
}
