using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Chimera/ChimeraHeavyBite")]
public class ChimeraHeavyBite : SkillBase
{
    [Tooltip("치명적 강타 계수")]
    public float damageMultiplier = 1.6f;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 맹렬한 물어뜯기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = Mathf.RoundToInt(user.CalculateDamage(target, skill) * damageMultiplier);
        target.TakeDamage(damage);

        Debug.Log($"<color=red>[괴수 강타]</color> {user.Unitname}의 거대한 아가리가 {target.Unitname}을(를) 물어뜯어 {damage}의 치명적 피해를 입혔습니다!");
    }
}
