using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Kraken/TentacleSweep")]
public class TentacleSweep : SkillBase
{
    [Header("기절 설정")]
    [Range(0, 100)]
    public int stunChance = 30;
    public int stunTurns = 1;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 촉수 휩쓸기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=red>[촉수 휩쓸기]</color> {user.Unitname}의 거대 촉수 휘두르기로 {target.Unitname}에게 {damage} 피해!");

        // 30% 확률 기절 (specialArmor 감쇄 적용)
        user.InflictStatusWithChance(target, StatusType.Stun, stunChance, stunTurns, 0);
    }
}
