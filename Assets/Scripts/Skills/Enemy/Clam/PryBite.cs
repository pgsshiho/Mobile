using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Clam/PryBite")]
public class PryBite : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 뜯어먹기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=orange>[뜯어먹기]</color> {user.Unitname}이(가) {target.Unitname}에게 {damage} 피해!");
    }
}
