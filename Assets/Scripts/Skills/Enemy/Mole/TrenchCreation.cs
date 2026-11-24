using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Mole/TrenchCreation")]
public class TrenchCreation : SkillBase
{
    public BuffData TrenchBuff;
    [Tooltip("참호 지속 턴 (기획: 2턴)")]
    public int DigdaDuration = 2;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        Unit recipient = target != null ? target : user;
        if (recipient == null) return;

        if (TrenchBuff != null)
        {
            recipient.AddBuff(TrenchBuff);
            Debug.Log($"{recipient.Unitname} 참호 구축 버프 적용!");
        }
    }
}
