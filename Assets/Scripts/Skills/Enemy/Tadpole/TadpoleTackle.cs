using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Tadpole/TadpoleTackle")]
public class TadpoleTackle : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 몸통 박치기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=cyan>[몸통 박치기]</color> {user.Unitname}이(가) {target.Unitname}에게 돌진하여 {damage} 피해!");
    }
}
