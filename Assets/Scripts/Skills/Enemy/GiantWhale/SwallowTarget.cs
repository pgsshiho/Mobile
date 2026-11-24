using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/GiantWhale/SwallowTarget")]
public class SwallowTarget : SkillBase
{
    [Tooltip("삼킨 지속 턴수")]
    public int swallowTurns = 2;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (user is GiantWhaleUnit whale)
        {
            whale.Swallow(target, swallowTurns);
        }
        else
        {
            Debug.LogWarning($"[삼키기] {user.Unitname}은(는) GiantWhaleUnit이 아닙니다.");
        }
    }
}
