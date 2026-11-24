using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Vulcan/FlashBang")]
public class FlashBang : SkillBase
{
    [Header("Stun Settings")]
    [Range(0, 100)]
    [Tooltip("기절 부여 확률 (기획: 70%, 대상의 specialArmor/special ammo에 의해 감쇄)")]
    public int stunChance = 70;

    [Tooltip("기절 지속 턴")]
    public int stunDuration = 1;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 섬광이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        // 공식: 상태이상 확률 = 유닛(스킬) 확률 - special ammo(specialArmor)
        int finalChance = Mathf.Clamp(stunChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            int appliedTurns = stunDuration > 0 ? stunDuration : (user != null ? user.stunInflictTurns : 1);
            target.AddStatus(StatusType.Stun, appliedTurns);
            Debug.Log($"{target.Unitname} 섬광에 맞아 기절! ({appliedTurns}턴, 성공률: {finalChance}%)");
        }
        else
        {
            Debug.Log($"{target.Unitname} 섬광 기절 저항 성공! (기절 확률 {stunChance}% - 특수장갑 {target.specialArmor}% = {finalChance}%)");
        }
    }
}
