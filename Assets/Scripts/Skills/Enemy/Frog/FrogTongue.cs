using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Frog/FrogTongue")]
public class FrogTongue : SkillBase
{
    [Header("산화 설정")]
    [Range(0, 100)]
    public int oxidationChance = 50;
    public int oxidationTurns = 3;
    public int oxidationDamage = 5;

    [Header("속도 감소 설정")]
    public int speedReduction = 5;
    public int speedDuration = 2;
    public DebuffData speedDebuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 낼름 공격이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=green>[낼름]</color> {user.Unitname}이(가) {target.Unitname}을 혀로 강타하여 {damage} 피해!");

        // 산화 부여
        user.InflictStatusWithChance(target, StatusType.OxidationI, oxidationChance, oxidationTurns, oxidationDamage);

        // 속도 5 감소
        if (speedDebuff != null)
        {
            target.AddDebuff(speedDebuff);
        }
        else
        {
            DebuffData tempDebuff = ScriptableObject.CreateInstance<DebuffData>();
            tempDebuff.debuffName = "낼름 감속";
            tempDebuff.duration = speedDuration;
            tempDebuff.speedPenalty = speedReduction;
            target.AddDebuff(tempDebuff);
        }
    }
}
