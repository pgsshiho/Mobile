using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/RedButterfly/PollenSpray")]
public class PollenSpray : SkillBase
{
    [Header("Oxidation Settings")]
    [Range(0, 100)]
    [Tooltip("산화 부여 확률 (공식: (확률 - special ammo)%)")]
    public int oxidationChance = 50;

    [Tooltip("산화 지속 턴 수")]
    public int oxidationTurns = 3;

    [Tooltip("산화 턴당 피해량 (방어력 무시)")]
    public int oxidationDamage = 5;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 꽃가루가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        // 공식: 상태이상 확률 = (유닛이 상태이상을 걸 숫자 - special ammo)%
        int finalChance = Mathf.Clamp(oxidationChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            target.AddOxidation(oxidationTurns, oxidationDamage);
            Debug.Log($"{target.Unitname} 꽃가루에 닿아 산화({oxidationTurns}턴, 턴당 {oxidationDamage}피해, 확률: {finalChance}%) 부여 완료!");
        }
        else
        {
            Debug.Log($"{target.Unitname} 꽃가루 산화 저항 성공! (확률 {oxidationChance}% - special ammo {target.specialArmor}% = {finalChance}%)");
        }
    }
}
