using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Centipede/CentipedeCoil")]
public class CentipedeCoil : SkillBase
{
    [Header("기절 설정 (확정 기절)")]
    [Tooltip("기절 부여 확률 (기본 100%)")]
    public int stunChance = 100;
    public int stunTurns = 1;

    [Header("속도 감소 디버프 설정")]
    [Tooltip("속도 감소치")]
    public int speedReduction = 1;
    public int speedDebuffDuration = 2;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 휘어감기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=red>[휘어감기]</color> {user.Unitname}이(가) {target.Unitname}을 휘어감아 {damage} 피해!");

        // 확정 기절 부여 (specialArmor 감쇄 공식 적용)
        user.InflictStatusWithChance(target, StatusType.Stun, stunChance, stunTurns, 0);

        // 속도 1 감소 디버프 부여
        DebuffData speedDebuff = ScriptableObject.CreateInstance<DebuffData>();
        speedDebuff.debuffName = "휘어감기 감속";
        speedDebuff.duration = speedDebuffDuration;
        speedDebuff.speedPenalty = speedReduction;
        target.AddDebuff(speedDebuff);

        Debug.Log($"<color=blue>[속도 감소]</color> {target.Unitname} 휘어감기로 속도 -{speedReduction} ({speedDebuffDuration}턴)");
    }
}
