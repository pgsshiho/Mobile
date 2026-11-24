using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Drowned/FishingHook")]
public class FishingHook : SkillBase
{
    [Header("확정 기절 설정")]
    [Tooltip("기절 부여 확률 (기본 100%)")]
    public int stunChance = 100;

    [Tooltip("기절 지속 턴수")]
    public int stunTurns = 1;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 낚시 스킬: 명중률 100 기반 (필중 혹은 기본 체크)
        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 낚시가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=cyan>[낚시]</color> {user.Unitname}이(가) {target.Unitname}을 낚아채 {damage} 피해!");

        // 확정 기절 (specialArmor 감쇄 적용)
        user.InflictStatusWithChance(target, StatusType.Stun, stunChance, stunTurns, 0);
    }
}
