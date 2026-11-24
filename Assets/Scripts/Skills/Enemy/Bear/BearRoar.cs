using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Bear/BearRoar")]
public class BearRoar : SkillBase
{
    [Header("Stun Settings")]
    [Range(0, 100)]
    public int stunChance = 50;
    public int stunTurns = 1;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 명중 여부와 상관없이 포효 충격파 전달
        int damage = Mathf.Max(1, Mathf.RoundToInt(user.attackPower * 0.5f));
        target.TakeDamage(damage);

        int finalChance = Mathf.Clamp(stunChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            target.AddStatus(StatusType.Stun, stunTurns);
            Debug.Log($"<color=yellow>[기절]</color> {target.Unitname} 곰의 맹렬한 포효에 압도당해 기절!");
        }
        else
        {
            Debug.Log($"{target.Unitname} 곰의 포효 저항 성공!");
        }
    }
}
