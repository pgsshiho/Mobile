using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/SeaHorse/SuckIn")]
public class SuckIn : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 빨아들이기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=cyan>[빨아들이기]</color> {user.Unitname}이(가) {target.Unitname}에게 {damage} 피해!");

        // 전열(인덱스 0)로 끌어오기
        if (PartyManager.instance != null && target is PlayerUnit player)
        {
            int currentIdx = PartyManager.instance.GetPlayerIndex(player);
            if (currentIdx > 0)
            {
                PartyManager.instance.SwapPartySlots(currentIdx, 0);
                Debug.Log($"<color=orange>[진형 교란]</color> {target.Unitname}이(가) 맨 앞으로 끌려옴!");
            }
        }
    }
}
