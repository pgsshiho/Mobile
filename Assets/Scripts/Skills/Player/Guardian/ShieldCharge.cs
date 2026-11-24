using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Guardian/ShieldCharge")]
public class ShieldCharge : SkillBase
{
    [Header("Stun Settings & Recoil")]
    [Range(0, 100)]
    [Tooltip("기절 부여 확률 (기획: 30%, 대상의 specialArmor/special ammo에 의해 감쇄)")]
    public int stunChance = 30;

    [Tooltip("기절 지속 턴")]
    public int stunDuration = 1;

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

        // 공식: 상태이상 확률 = 유닛(스킬) 확률 - special ammo(specialArmor)
        int finalChance = Mathf.Clamp(stunChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            int appliedTurns = stunDuration > 0 ? stunDuration : (user != null ? user.stunInflictTurns : 1);
            target.AddStatus(StatusType.Stun, appliedTurns);
            Debug.Log($"{target.Unitname} 돌진 충격으로 기절! ({appliedTurns}턴, 성공률: {finalChance}%)");
        }
        else
        {
            Debug.Log($"{target.Unitname} 돌진 기절 저항 성공! (기절 확률 {stunChance}% - 특수장갑 {target.specialArmor}% = {finalChance}%)");
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
