using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/WeakAttack")]
public class WeakAttack : SkillBase
{
    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        if (user == null || target == null) return;

        // 명중 체크
        if (!user.CheckHit(target, skill))
        {
            Debug.Log("빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);

        // 상태이상(모든 디버프/상태이상)이 걸려있을 경우 1.5배 피해
        if (target.HasAnyStatus())
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            Debug.Log($"[약점 타격] {target.Unitname} 상태이상 보유로 1.5배 증폭 피해! ({damage})");
        }

        target.TakeDamage(damage);
    }
}
