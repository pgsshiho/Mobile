using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Clam/ClamWaterSpit")]
public class ClamWaterSpit : SkillBase
{
    [Header("산화 설정")]
    [Range(0, 100)]
    [Tooltip("산화 부여 기본 확률")]
    public int oxidationChance = 50;

    [Tooltip("산화 지속 턴")]
    public int oxidationTurns = 3;

    [Tooltip("산화 턴당 피해량")]
    public int oxidationDamage = 5;

    [Header("속도 디버프 설정")]
    [Tooltip("속도 감소치")]
    public int speedReduction = 10;

    [Tooltip("연결할 DebuffData (Inspector에서 할당 가능)")]
    public DebuffData speedDebuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 조갯물 뱉기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=cyan>[조갯물 뱉기]</color> {user.Unitname}이(가) {target.Unitname}에게 {damage} 피해!");

        // 산화 부여 (specialArmor 저항 공식)
        user.InflictStatusWithChance(target, StatusType.OxidationI, oxidationChance, oxidationTurns, oxidationDamage);

        // 속도 10 감소 디버프
        if (speedDebuff != null)
        {
            target.AddDebuff(speedDebuff);
            Debug.Log($"<color=blue>[속도 감소]</color> {target.Unitname} 속도 감소 디버프 적용!");
        }
        else
        {
            DebuffData tempDebuff = ScriptableObject.CreateInstance<DebuffData>();
            tempDebuff.debuffName = "조갯물 속도저하";
            tempDebuff.duration = 2;
            tempDebuff.speedPenalty = speedReduction;
            target.AddDebuff(tempDebuff);
            Debug.Log($"<color=blue>[속도 감소]</color> {target.Unitname} 속도 -{speedReduction} (2턴)");
        }
    }
}
