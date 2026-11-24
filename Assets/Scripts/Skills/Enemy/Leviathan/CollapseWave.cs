using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Leviathan/CollapseWave")]
public class CollapseWave : SkillBase
{
    [Header("방어력 감소 디버프 설정")]
    public int defensePenalty = 15;
    public int duration = 3;
    public DebuffData defenseDebuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 붕괴 파동이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=purple>[붕괴 파동]</color> {user.Unitname}의 차원 붕괴 파동이 {target.Unitname}을 강타하여 {damage} 피해!");

        // 방어력 15 감소
        if (defenseDebuff != null)
        {
            target.AddDebuff(defenseDebuff);
        }
        else
        {
            DebuffData tempDebuff = ScriptableObject.CreateInstance<DebuffData>();
            tempDebuff.debuffName = "장갑 붕괴";
            tempDebuff.duration = duration;
            tempDebuff.defensePenalty = defensePenalty;
            target.AddDebuff(tempDebuff);
        }
    }
}
