using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Eagle/WeakpointSnipe")]
public class WeakpointSnipe : SkillBase
{
    [Header("Bonus Crit Chance")]
    public float bonusCrit = 35f;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 약점 저격이 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);

        // 추가 크리티컬 롤
        if (Random.Range(0, 100) < bonusCrit)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            Debug.Log("약점 정밀 저격 크리티컬 성공!");
        }

        target.TakeDamage(damage);
        TryApplyStatus(target, skill);
        Debug.Log($"{user.Unitname} 이(가) {target.Unitname} 에게 {damage} 약점 저격 피해!");
    }
}
