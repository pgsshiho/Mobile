using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Leviathan/AbsolutePredation")]
public class AbsolutePredation : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 절대 포식이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=red><b>[절대 포식 발동]</b></color> {user.Unitname}의 심연의 절대 포식이 {target.Unitname}에게 {damage} 파멸적 피해를 입혔습니다!");
    }
}
