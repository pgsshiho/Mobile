using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Laptor/Divide")]
public class Divide : SkillBase
{
    public override void Use(
        Unit user,
        Unit target,
        SkillData skill
    )
    {
        if (user == null || target == null) return;

        // 전열(1열)로 이동
        MoveToFront(user);

        // 명중 체크
        if (!user.CheckHit(target, skill))
        {
            Debug.Log("빗나감!");
            return;
        }

        // 데미지 계산
        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        Debug.Log(user.name + " -> " + target.name + " 가르기 " + damage);

        // 사망 체크
        if (target.health <= 0)
        {
            target.Die();
        }
    }

    private void MoveToFront(Unit user)
    {
        if (PartyManager.instance == null) return;

        var slots = PartyManager.instance.partySlots;
        if (slots == null) return;

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

            if (BattleManager.instance != null && BattleManager.instance.Party != null)
            {
                PartyManager.instance.PlacePartyAtPositions(BattleManager.instance.Party);
            }

            PartyManager.instance.SaveParty();
            Debug.Log($"[가르기] {user.Unitname} 전열(1열)로 이동 완료");
        }
    }
}
