using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Chimera/ChimeraRoar")]
public class ChimeraRoar : SkillBase
{
    [Header("Stun Settings")]
    [Range(0, 100)]
    public int stunChance = 70;
    public int stunTurns = 1;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        int damage = Mathf.Max(1, Mathf.RoundToInt(user.attackPower * 0.4f));
        target.TakeDamage(damage);

        int finalChance = Mathf.Clamp(stunChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            target.AddStatus(StatusType.Stun, stunTurns);
            Debug.Log($"<color=yellow>[공포의 포효]</color> {target.Unitname} 키메라의 괴성에 짓눌려 기절!");
        }
        else
        {
            Debug.Log($"{target.Unitname} 키메라의 포효 저항 성공!");
        }
    }
}
