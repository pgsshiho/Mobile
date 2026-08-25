using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Overcharge")]
public class Overcharge : SkillBase
{
    public BuffData overchargeBuff;

    [Range(0, 100)]
    public int overheatChance = 30;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        if (user == null) return;

        if (overchargeBuff != null)
        {
            user.AddBuff(overchargeBuff);
        }

        if (Random.Range(0, 100) < overheatChance)
        {
            user.AddStatus(StatusType.Overheat, 3);
            Debug.Log($"{user.Unitname} 오버차지 반동으로 과열 발생!");
        }
    }
}