using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Hornet/AttackDeploy")]
public class AttackDeploy : SkillBase
{
    [Header("DOT Settings")]
    [Range(0, 100)]
    [Tooltip("지속 피해 부여 확률 (기획: 100%, 대상의 specialArmor/special ammo에 의해 감쇄)")]
    public int dotChance = 100;

    [Tooltip("지속 피해 지속 턴")]
    public int dotDuration = 3;

    [Tooltip("지속 피해 턴당 피해량 (기획: 5)")]
    public int dotDamage = 5;

    [Header("Debuff")]
    public DebuffData attackDeployDebuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 공격 드론 배치가 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        // 공식: 상태이상 확률 = 유닛(스킬) 확률 - special ammo(specialArmor)
        int finalChance = Mathf.Clamp(dotChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            int appliedDamage = dotDamage > 0 ? dotDamage : (user != null ? user.bleedInflictDamage : 5);
            target.AddStatus(StatusType.Bleeding, dotDuration, appliedDamage);
            Debug.Log($"{target.Unitname} 공격 드론 부착 완료! 지속 피해({dotDuration}턴, 턴당 {appliedDamage}피해, 확률: {finalChance}%) 적용!");
        }

        if (attackDeployDebuff != null)
        {
            target.AddDebuff(attackDeployDebuff);
        }
    }
}
