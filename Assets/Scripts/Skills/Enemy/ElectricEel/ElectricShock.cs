using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/ElectricEel/ElectricShock")]
public class ElectricShock : SkillBase
{
    [Header("기절 설정 (전기 충격 - 50% 기절 확률)")]
    [Range(0, 100)]
    [Tooltip("기절 부여 확률 (공식: 기절확률 - target.specialArmor)")]
    public int stunChance = 50;

    [Tooltip("기절 지속 턴수")]
    public int stunTurns = 1;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 전기 충격이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=cyan>[전기 충격]</color> {user.Unitname}이(가) {target.Unitname}에게 {damage} 피해!");

        int finalChance = Mathf.Clamp(stunChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            target.AddStatus(StatusType.Stun, stunTurns);
            Debug.Log($"<color=yellow>[기절]</color> {target.Unitname} 전기 충격으로 기절! ({stunTurns}턴, 확률: {finalChance}%)");
        }
        else
        {
            Debug.Log($"{target.Unitname} 기절 저항 성공! (기본확률 {stunChance}% - special armor {target.specialArmor}% = {finalChance}%)");
        }
    }
}
