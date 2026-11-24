using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Kraken/KrakenHide")]
public class KrakenHide : SkillBase
{
    [Tooltip("체력 회복량")]
    public int healAmount = 20;

    [Tooltip("무적 지속 턴수")]
    public int invulnerableTurns = 1;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        Unit self = target != null ? target : user;
        if (self == null) return;

        // 1턴간 무적 적용
        self.isInvulnerable = true;
        self.invulnerableTurns = Mathf.Max(self.invulnerableTurns, invulnerableTurns);

        // 체력 20 회복
        float prevHp = self.health;
        self.health = Mathf.Min(self.maxHealth, self.health + healAmount);
        float healed = self.health - prevHp;
        self.UpdateHealthBar();

        Debug.Log($"<color=cyan>[은신]</color> {self.Unitname}이(가) 깊은 심해로 은신하여 1턴간 무적 상태가 되고 체력을 {healed} 회복했습니다! ({self.health}/{self.maxHealth})");
    }
}
