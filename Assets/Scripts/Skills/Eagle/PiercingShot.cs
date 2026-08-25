using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Eagle/PiercingShot")]
public class PiercingShot : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 관통 사격이 {target.Unitname}에게 빗나감!");
            return;
        }

        // 방어력 무시 관통 데미지 계산
        int rawDamage = user.GetAttackPower() + skill.power;
        rawDamage = Mathf.Max(1, rawDamage);

        target.TakeDamage(rawDamage);
        TryApplyStatus(target, skill);
        Debug.Log($"{user.Unitname} 관통 사격! 방어력을 무시하고 {target.Unitname}에게 {rawDamage} 피해!");
    }
}
