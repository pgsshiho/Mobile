using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Leviathan/CellDivision")]
public class CellDivision : SkillBase
{
    [Tooltip("체력 회복량")]
    public int healAmount = 30;

    [Tooltip("다음 공격력 증가량")]
    public int attackBonus = 15;

    [Tooltip("공격력 버프 지속 턴")]
    public int buffDuration = 2;

    [Tooltip("연결할 BuffData (Inspector에서 할당 가능)")]
    public BuffData attackBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        Unit self = target != null ? target : user;
        if (self == null) return;

        // 체력 30 회복
        float prevHp = self.health;
        self.health = Mathf.Min(self.maxHealth, self.health + healAmount);
        float healed = self.health - prevHp;
        self.UpdateHealthBar();

        // 다음 공격력 15 증가 버프
        if (attackBuff != null)
        {
            self.AddBuff(attackBuff);
        }
        else
        {
            BuffData tempBuff = ScriptableObject.CreateInstance<BuffData>();
            tempBuff.buffName = "세포 증식";
            tempBuff.duration = buffDuration;
            tempBuff.attackBonus = attackBonus;
            self.AddBuff(tempBuff);
        }

        Debug.Log($"<color=green>[세포 분열]</color> {self.Unitname}의 세포가 급속 분열하여 체력 {healed} 회복 및 공격력 {attackBonus} 증가!");
    }
}
