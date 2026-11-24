using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Drowned/WaterGhostReady")]
public class WaterGhostReady : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (user is DrownedUnit drowned)
        {
            drowned.isWaterGhostReady = true;
        }

        Debug.Log($"<color=red>[물귀신 준비]</color> {user.Unitname}이(가) 깊은 물 속에서 물귀신 시전을 준비합니다! (다음 턴 즉사기 사용 가능)");
    }
}
