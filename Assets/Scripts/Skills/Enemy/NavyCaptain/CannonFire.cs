using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/NavyCaptain/CannonFire")]
public class CannonFire : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (user is NavyCaptainUnit captain)
        {
            captain.needsReload = true;
        }

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 함포 사격이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=red>[대포발사]</color> {user.Unitname}의 거대 함포가 폭발하며 {target.Unitname}에게 {damage} 치명적 피해!");
    }
}
