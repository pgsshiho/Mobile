using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Cerberus/CerberusPounce")]
public class CerberusPounce : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 달려들기가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        // 파티 진형 충격: 대상의 위치 교란
        if (PartyManager.instance != null && target is PlayerUnit player)
        {
            int currentIdx = PartyManager.instance.GetPlayerIndex(player);
            if (currentIdx >= 0)
            {
                int newIdx = Mathf.Clamp(currentIdx + 1, 0, PartyManager.instance.partySlots.Length - 1);
                if (newIdx != currentIdx)
                {
                    PartyManager.instance.SwapPartySlots(currentIdx, newIdx);
                    Debug.Log($"<color=orange>[진형 붕괴]</color> 달려들기 충격으로 {target.Unitname}이(가) 뒤로 밀려남!");
                }
            }
        }

        Debug.Log($"{user.Unitname} 달려들기 적중! {target.Unitname}에게 {damage} 피해!");
    }
}
