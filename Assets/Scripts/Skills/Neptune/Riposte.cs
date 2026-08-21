using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Neptune/Riposte")]
public class Riposte : SkillBase
{
    [Header("Buff")]
    public BuffData riposteBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (riposteBuff != null)
        {
            user.AddBuff(riposteBuff);
        }

        Debug.Log($"{user.Unitname} 반격 태세 준비! 1턴간 피격 시 즉시 반격!");
    }
}
