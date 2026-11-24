using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/SeaHorse/TailCoil")]
public class TailCoil : SkillBase
{
    [Header("속도 감소 설정")]
    [Tooltip("속도 감소치")]
    public int speedReduction = 10;

    [Tooltip("디버프 지속 턴")]
    public int duration = 2;

    [Tooltip("연결할 DebuffData (Inspector에서 할당 가능)")]
    public DebuffData speedDebuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 꼬리 감기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=cyan>[꼬리 감기]</color> {user.Unitname}이(가) {target.Unitname}에게 {damage} 피해!");

        if (speedDebuff != null)
        {
            target.AddDebuff(speedDebuff);
            Debug.Log($"<color=blue>[속도 감소]</color> {target.Unitname} 꼬리 감기로 속도 감소 디버프!");
        }
        else
        {
            DebuffData tempDebuff = ScriptableObject.CreateInstance<DebuffData>();
            tempDebuff.debuffName = "꼬리감기 속도저하";
            tempDebuff.duration = duration;
            tempDebuff.speedPenalty = speedReduction;
            target.AddDebuff(tempDebuff);
            Debug.Log($"<color=blue>[속도 감소]</color> {target.Unitname} 속도 -{speedReduction} ({duration}턴)");
        }
    }
}
