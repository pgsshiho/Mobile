using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Hornet/HornetShot")]
public class HornetShot : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 사격이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        Debug.Log($"{user.Unitname} 이(가) {target.Unitname} 에게 {damage} 사격 피해!");
    }
}
