using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/RedPupa/PupaProtect")]
public class PupaProtect : SkillBase
{
    [Header("Buff")]
    [Tooltip("아군 대신 맞기 / 도발 / 방어 버프")]
    public BuffData protectBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (protectBuff != null)
        {
            user.AddBuff(protectBuff);
        }

        Debug.Log($"{user.Unitname} 보호 발동! 아군을 지키기 위해 자세를 굳힙니다.");
    }
}
