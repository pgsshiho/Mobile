using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Cerberus/CerberusBerserk")]
public class CerberusBerserk : SkillBase
{
    [Header("Buff")]
    public BuffData berserkBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (berserkBuff != null)
        {
            user.AddBuff(berserkBuff);
        }

        Debug.Log($"<color=red>[광폭화 가동]</color> {user.Unitname}의 눈이 붉게 빛나며 공격력과 속도가 대폭 상승합니다!");
    }
}
