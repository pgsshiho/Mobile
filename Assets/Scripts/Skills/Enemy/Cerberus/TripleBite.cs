using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Cerberus/TripleBite")]
public class TripleBite : SkillBase
{
    [Tooltip("기본 공격 횟수")]
    public int baseHitCount = 3;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 체력이 낮을수록 추가 타격 (최대 5회)
        int hitCount = baseHitCount;
        if (user.health <= user.maxHealth * 0.5f) hitCount += 1;
        if (user.health <= user.maxHealth * 0.25f) hitCount += 1;

        int totalDamage = 0;
        int successfulHits = 0;

        for (int i = 0; i < hitCount; i++)
        {
            if (target.health <= 0) break;

            if (user.CheckHit(target, skill))
            {
                int damage = Mathf.Max(1, user.CalculateDamage(target, skill));
                target.TakeDamage(damage);
                totalDamage += damage;
                successfulHits++;
            }
        }

        Debug.Log($"<color=red>[삼중 포식]</color> {user.Unitname}이(가) {target.Unitname}을(를) {successfulHits}/{hitCount}회 연속 타격! (총 피해: {totalDamage})");
    }
}
