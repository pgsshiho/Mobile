using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/GiantSpider/Cocooning")]
public class Cocooning : SkillBase
{
    [Header("Stun Settings")]
    [Range(0, 100)]
    public int stunChance = 100;
    public int stunTurns = 1;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        int finalChance = Mathf.Clamp(stunChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            target.AddStatus(StatusType.Stun, stunTurns);
            Debug.Log($"<color=yellow>[고치 포박]</color> {target.Unitname}이(가) 거미줄 고치에 묶여 {stunTurns}턴간 완전히 무력화(기절)되었습니다!");
        }
        else
        {
            Debug.Log($"{target.Unitname} 거미줄 고치 포박 저항 성공!");
        }
    }
}
