using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Eagle/DataPollutionBullet")]
public class DataPollutionBullet : SkillBase
{
    [Header("Pollution Settings")]
    [Range(0, 100)]
    [Tooltip("오염 상태 부여 확률 (기획: 100%, 대상의 specialArmor/special ammo에 의해 감쇄)")]
    public int pollutionChance = 100;

    [Tooltip("오염 지속 턴")]
    public int duration = 3;

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

        // 공식: 상태이상 확률 = 유닛(스킬) 확률 - special ammo(specialArmor)
        int finalChance = Mathf.Clamp(pollutionChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            target.AddStatus(StatusType.WeaponPollution, duration);
            Debug.Log($"{target.Unitname} 데이터 오염탄 피격! 무장 오염 발생! (성공률: {finalChance}%)");
        }
        else
        {
            Debug.Log($"{target.Unitname} 데이터 오염 저항 성공! (확률 {pollutionChance}% - 특수장갑 {target.specialArmor}% = {finalChance}%)");
        }
    }
}
