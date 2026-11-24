using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Parasyte/SpittingAcid")]
public class SpittingAcid : SkillBase
{
    [Header("Status Effect (산성뱉기 2인 공격 산화확률 30%)")]
    [Range(0, 100)]
    [Tooltip("산화 부여 확률 (기획: 30%, 공식: (유닛 확률 - special ammo)%)")]
    public int oxidationChance = 30;

    [Tooltip("산화 지속 턴수")]
    public int oxidationDuration = 3;

    [Tooltip("산화 턴당 피해량 (방어력 무시)")]
    public int oxidationDamage = 5;

    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 산성뱉기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        // 공식: 상태이상 확률 = (유닛이 상태이상을 걸 숫자 - special ammo)%
        int finalChance = Mathf.Clamp(oxidationChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            int appliedTurns = oxidationDuration > 0 ? oxidationDuration : (user != null ? user.oxidationTurns : 3);
            int appliedDamage = oxidationDamage > 0 ? oxidationDamage : (user != null ? user.oxidationDamage : 5);
            target.AddOxidation(appliedTurns, appliedDamage);
            Debug.Log($"{target.Unitname} 산성 침식으로 산화 부여 완료! ({appliedTurns}턴, 턴당 {appliedDamage}피해, 확률: {finalChance}%)");
        }
        else
        {
            Debug.Log($"{target.Unitname} 산성뱉기 산화 저항 성공! (기본확률 {oxidationChance}% - special ammo {target.specialArmor}% = {finalChance}%)");
        }
    }
}
