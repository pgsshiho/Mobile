using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Hornet/DroneCharge")]
public class DroneCharge : SkillBase
{
    [Header("Drone Buff")]
    public BuffData droneBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (droneBuff != null)
        {
            user.AddBuff(droneBuff);
        }

        Debug.Log($"{user.Unitname} 드론 충전 완료!");
    }
}
