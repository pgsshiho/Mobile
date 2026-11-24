using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/NavyCaptain/Reorganize_NC")]
public class Reorganize_NC : SkillBase
{
    [Tooltip("체력 회복량")]
    public int healAmount = 30;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        Unit self = target != null ? target : user;
        if (self == null) return;

        float prevHp = self.health;
        self.health = Mathf.Min(self.maxHealth, self.health + healAmount);
        float healed = self.health - prevHp;
        self.UpdateHealthBar();

        Debug.Log($"<color=green>[재정비]</color> {self.Unitname}이(가) 숨을 고르며 체력을 {healed} 회복했습니다! ({self.health}/{self.maxHealth})");
    }
}
