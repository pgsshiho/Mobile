using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Vulcan/FlashBang")]
public class FlashBang : SkillBase
{
    [Header("Stun Chance")]
    [Range(0, 100)]
    public int stunChance = 50;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 섬광이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        if (Random.Range(0, 100) < stunChance)
        {
            target.isStunned = true;
            Debug.Log($"{target.Unitname} 섬광에 맞아 기절!");
        }
    }
}
