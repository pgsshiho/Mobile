using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/GiantSpider/SpiderPoisonSpray")]
public class SpiderPoisonSpray : SkillBase
{
    [Header("Oxidation (Poison) Settings")]
    [Range(0, 100)]
    public int oxidationChance = 70;
    public int oxidationTurns = 3;
    public int oxidationDamage = 6;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        int finalChance = Mathf.Clamp(oxidationChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            target.AddOxidation(oxidationTurns, oxidationDamage);
            Debug.Log($"<color=green>[부식성 맹독]</color> {target.Unitname} 거대거미의 산성 독액에 맞아 산화 부여!");
        }
    }
}
