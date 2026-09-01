using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Neptune/PropulsionStrike")]
public class PropulsionStrike : SkillBase
{
    [Header("Max HP Damage Ratio")]
    public float hpDamageRatio = 0.05f;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        MoveToFront(user);

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 추진 타격이 {target.Unitname}에게 빗나감!");
            return;
        }

        int baseDamage = user.CalculateDamage(target, skill);
        int hpBonusDamage = 0;
        int selfDamage = Mathf.RoundToInt(user.maxHealth * hpDamageRatio);
        user.TakeDamage(selfDamage);
        int finalDamage = baseDamage + hpBonusDamage;

        target.TakeDamage(finalDamage);
        Debug.Log($"{user.Unitname} 추진 타격! 체력 비례 총 {finalDamage} 피해!");
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
