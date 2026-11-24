using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/AncientTreeAlien/TreeTargetDesignation")]
public class TreeTargetDesignation : SkillBase
{
    [Header("Mark Settings (2명 지정)")]
    [Range(0, 100)]
    public int markChance = 100;
    public int markDuration = 3;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        int finalChance = Mathf.Clamp(markChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            target.AddStatus(StatusType.Marked, markDuration);
            Debug.Log($"<color=red>[외계 촉수 표식]</color> {target.Unitname}에게 외계생명체의 붉은 촉수 표식이 새겨졌습니다! ({markDuration}턴)");
        }
    }
}
