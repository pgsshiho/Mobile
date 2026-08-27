using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Hornet/AttackDeploy")]
public class AttackDeploy : SkillBase
{
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

        // 지속 데미지 (출혈 3턴)
        target.AddStatus(StatusType.Bleeding, 3);
        if (attackDeployDebuff != null)
        {
            target.AddDebuff(attackDeployDebuff);
        }

        Debug.Log($"{target.Unitname} 공격 드론 부착 완료 (지속 피해 적용)!");
    }
}
