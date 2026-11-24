using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Centipede/CentipedePoisonSpray")]
public class CentipedePoisonSpray : SkillBase
{
    [Header("산화 부여 설정")]
    [Range(0, 100)]
    public int oxidationChance = 60;
    public int oxidationTurns = 3;
    public int oxidationDamage = 5;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 명중 체크
        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 독 뿌리기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=green>[독 뿌리기]</color> {user.Unitname}이(가) {target.Unitname}에게 독액을 뿌려 {damage} 피해!");

        // 산화 부여 (specialArmor 감쇄 공식 적용)
        user.InflictStatusWithChance(target, StatusType.OxidationI, oxidationChance, oxidationTurns, oxidationDamage);
    }
}
