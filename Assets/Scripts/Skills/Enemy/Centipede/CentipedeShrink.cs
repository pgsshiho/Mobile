using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Centipede/CentipedeShrink")]
public class CentipedeShrink : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user is CentipedeUnit centipede)
        {
            centipede.MergeIntoOne();
        }
        else if (user is CentipedeSegmentUnit segment && segment.mainBody != null)
        {
            segment.mainBody.MergeIntoOne();
        }
        else
        {
            Debug.LogWarning("[줄어들기] 시전자 정보를 찾을 수 없습니다.");
        }
    }
}
