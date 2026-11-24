using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Drowned/WaterGhostInstakill")]
public class WaterGhostInstakill : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 물귀신 사용 후 준비 상태 해제
        if (user is DrownedUnit drowned)
        {
            drowned.isWaterGhostReady = false;
        }

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 물귀신이 {target.Unitname}에게 빗나감!");
            return;
        }

        // 조건: 대상의 현재 체력이 최대 체력의 1/3 이하라면 확정 처치. 기준 체력이 20보다 낮으면 20을 기준으로 함.
        int threshold = Mathf.Max(20, Mathf.RoundToInt(target.maxHealth / 3f));

        if (target.health <= threshold)
        {
            Debug.Log($"<color=red>[물귀신 발동 - 처형!]</color> {target.Unitname}의 체력({target.health})이 기준치({threshold}) 이하이므로 즉시 끌고 들어갑니다!");
            target.TakeDamage(Mathf.CeilToInt(target.health), Unit.DamageType.Status);
        }
        else
        {
            // 기준치 초과 시 통상 강력 피해
            int damage = user.CalculateDamage(target, skill);
            target.TakeDamage(damage);
            Debug.Log($"<color=orange>[물귀신 실패]</color> {target.Unitname}의 체력이 기준치({threshold})보다 높아 즉사 실패, {damage} 피해만 입힘!");
        }
    }
}
