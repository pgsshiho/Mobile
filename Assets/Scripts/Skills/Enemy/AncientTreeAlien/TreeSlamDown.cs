using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/AncientTreeAlien/TreeSlamDown")]
public class TreeSlamDown : SkillBase
{
    [Header("Damage Settings")]
    public float normalDamageMultiplier = 1.3f;
    public float markedDamageMultiplier = 2.5f;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 거대 가지 내려치기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int baseDmg = user.CalculateDamage(target, skill);
        bool isMarked = target.isMarked || target.HasStatus(StatusType.Marked);

        float mult = isMarked ? markedDamageMultiplier : normalDamageMultiplier;
        int finalDmg = Mathf.RoundToInt(baseDmg * mult);

        target.TakeDamage(finalDmg);

        if (isMarked)
        {
            Debug.Log($"<color=red>[표식 강타!!]</color> {user.Unitname}이(가) 타겟 지정된 {target.Unitname}에게 궤멸적인 내려치기를 가해 {finalDmg} 피해!");
        }
        else
        {
            Debug.Log($"{user.Unitname}의 내려치기! {target.Unitname}에게 {finalDmg} 피해!");
        }
    }
}
