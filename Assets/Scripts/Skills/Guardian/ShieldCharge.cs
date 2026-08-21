using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Guardian/ShieldCharge")]
public class ShieldCharge : SkillBase
{
    [Header("Stun Chance & Recoil")]
    [Range(0, 100)]
    public int stunChance = 50;
    public int selfDamage = 8;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 전열(1열)로 이동
        MoveToFront(user);

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 돌진이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        if (Random.Range(0, 100) < stunChance)
        {
            target.isStunned = true;
            Debug.Log($"{target.Unitname} 돌진 충격으로 기절!");
        }

        // 반동 피해
        user.TakeDamage(selfDamage, Unit.DamageType.Normal);
    }

    private void MoveToFront(Unit user)
    {
        if (PartyManager.instance == null) return;

        var slots = PartyManager.instance.partySlots;
        int currentIndex = -1;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == user)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex > 0)
        {
            Unit movingUnit = slots[currentIndex];
            for (int i = currentIndex; i > 0; i--)
            {
                slots[i] = slots[i - 1];
            }
            slots[0] = movingUnit;
            PartyManager.instance.SaveParty();
        }
    }
}
