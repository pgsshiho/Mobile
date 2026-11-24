using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/NavyCaptain/DeckDefense")]
public class DeckDefense : SkillBase
{
    [Header("방어력 버프 설정")]
    [Tooltip("방어력 증가량 (기획: 20)")]
    public int defenseBonus = 20;

    [Tooltip("지속 턴수 (기획: 2턴)")]
    public int duration = 2;

    [Tooltip("연결할 BuffData (Inspector에서 할당 가능)")]
    public BuffData defenseBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        Unit recipient = target != null ? target : user;
        if (recipient == null) return;

        if (defenseBuff != null)
        {
            recipient.AddBuff(defenseBuff);
        }
        else
        {
            BuffData tempBuff = ScriptableObject.CreateInstance<BuffData>();
            tempBuff.buffName = "갑판 방어";
            tempBuff.duration = duration;
            tempBuff.defenseBonus = defenseBonus;
            recipient.AddBuff(tempBuff);
        }

        Debug.Log($"<color=blue>[갑판 방어]</color> {recipient.Unitname}의 방어력이 {defenseBonus} 증가했습니다! ({duration}턴)");
    }
}
