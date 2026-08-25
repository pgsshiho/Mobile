using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Neptune/ShieldBash")]
public class ShieldBash : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        MoveToFront(user);

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 방패 돌진이 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        TryApplyStatus(target, skill);

        Debug.Log($"{user.Unitname} 방패 돌진 적중! {target.Unitname}에게 {damage} 피해 및 넉백!");
    }

    private void MoveToFront(Unit user)
    {
        if (PartyManager.instance == null) return;
        var slots = PartyManager.instance.partySlots;
        int currentIndex = -1;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == user) { currentIndex = i; break; }
        }
        if (currentIndex > 0)
        {
            Unit movingUnit = slots[currentIndex];
            for (int i = currentIndex; i > 0; i--) { slots[i] = slots[i - 1]; }
            slots[0] = movingUnit;
            PartyManager.instance.SaveParty();
        }
    }
}
