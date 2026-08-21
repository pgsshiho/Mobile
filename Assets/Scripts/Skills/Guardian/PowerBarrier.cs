using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Guardian/PowerBarrier")]
public class PowerBarrier : SkillBase
{
    [Header("Buff")]
    public BuffData barrierBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (target == null) return;

        if (barrierBuff != null)
        {
            target.AddBuff(barrierBuff);
        }

        Debug.Log($"{target.Unitname} 전력 방벽 적용! 방어력 20% 상승!");
    }
}
