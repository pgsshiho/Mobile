using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Eagle/DataPollutionBullet")]
public class DataPollutionBullet : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 데이터 오염탄이 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        // 무장 오염 상태 부여
        target.AddStatus(StatusType.WeaponPollution);
        Debug.Log($"{target.Unitname} 데이터 오염탄 피격! 무장 오염 발생!");
    }
}
