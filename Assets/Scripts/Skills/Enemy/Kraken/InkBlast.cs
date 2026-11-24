using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Kraken/InkBlast")]
public class InkBlast : SkillBase
{
    [Header("명중률 감소 디버프 설정")]
    public int hitPenalty = 15;
    public int duration = 2;
    public DebuffData accuracyDebuff;

    [Header("오염 상태이상 설정")]
    [Range(0, 100)]
    public int pollutionChance = 60;
    public int pollutionTurns = 3;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 먹물 분사가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=black>[먹물 분사]</color> {user.Unitname}의 먹물 분사로 {target.Unitname}에게 {damage} 피해!");

        // 명중률 15 감소 디버프
        if (accuracyDebuff != null)
        {
            target.AddDebuff(accuracyDebuff);
        }
        else
        {
            DebuffData tempDebuff = ScriptableObject.CreateInstance<DebuffData>();
            tempDebuff.debuffName = "먹물 시야차단";
            tempDebuff.duration = duration;
            tempDebuff.hitPenalty = hitPenalty;
            target.AddDebuff(tempDebuff);
        }

        // 오염(무기 오염) 부여
        user.InflictStatusWithChance(target, StatusType.WeaponPollution, pollutionChance, pollutionTurns, 0);
    }
}
