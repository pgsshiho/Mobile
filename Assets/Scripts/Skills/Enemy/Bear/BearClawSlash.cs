using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Bear/BearClawSlash")]
public class BearClawSlash : SkillBase
{
    [Header("Oxidation Settings")]
    [Range(0, 100)]
    public int oxidationChance = 60;
    public int oxidationTurns = 3;
    public int oxidationDamage = 5;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 할퀴기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        int finalChance = Mathf.Clamp(oxidationChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            target.AddOxidation(oxidationTurns, oxidationDamage);
            Debug.Log($"{target.Unitname} 곰의 거대한 발톱에 찢겨 산화 부여!");
        }
    }
}
