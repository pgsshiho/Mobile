using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/RedPupa/PupaEndure")]
public class PupaEndure : SkillBase
{
    [Header("Endure Buff (방어력 5 상승 2턴)")]
    public BuffData endureBuff;
    public int defenseIncrease = 5;
    public int duration = 2;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (endureBuff != null)
        {
            user.AddBuff(endureBuff);
        }

        Debug.Log($"{user.Unitname} 견디기 발동! 방어력 {defenseIncrease} 상승 ({duration}턴)!");
    }
}
