using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Leviathan/ErosionTentacle")]
public class ErosionTentacle : SkillBase
{
    [Header("오염 설정")]
    [Range(0, 100)]
    public int pollutionChance = 70;
    public int pollutionTurns = 3;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 침식 촉수가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=purple>[침식 촉수]</color> {user.Unitname}의 침식 촉수가 {target.Unitname}을 휘감아 {damage} 피해!");

        // 오염(무기오염) 부여 (specialArmor 감쇄 적용)
        user.InflictStatusWithChance(target, StatusType.WeaponPollution, pollutionChance, pollutionTurns, 0);
    }
}
