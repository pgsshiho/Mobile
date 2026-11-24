using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Egg/EggWobble")]
public class EggWobble : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        Debug.Log($"<color=white>[흔들거리기]</color> {user?.Unitname ?? "알"}이(가) 가만히 흔들거립니다... (아무 일도 일어나지 않음)");
    }
}
