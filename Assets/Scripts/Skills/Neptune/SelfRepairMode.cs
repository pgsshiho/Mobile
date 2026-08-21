using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Neptune/SelfRepairMode")]
public class SelfRepairMode : SkillBase
{
    [Header("Heal & Buff")]
    public BuffData defensePenaltyBuff;
    public int nextTurnHealAmount = 40;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        // 1칸 뒤로 이동
        MoveBackOneSlot(user);

        // 방어력 감소 디버프/버프 적용
        if (defensePenaltyBuff != null)
        {
            user.AddBuff(defensePenaltyBuff);
        }

        // 즉시/다음 턴 치유
        user.Heal(Mathf.RoundToInt(nextTurnHealAmount * user.GetHealMultiplier()));

        Debug.Log($"{user.Unitname} 자가 수리 모드 가동! 뒤로 1칸 후퇴하며 긴급 복구 진행!");
    }

    private void MoveBackOneSlot(Unit user)
    {
        if (PartyManager.instance == null) return;
        var slots = PartyManager.instance.partySlots;
        for (int i = 0; i < slots.Length - 1; i++)
        {
            if (slots[i] == user && slots[i + 1] != null)
            {
                Unit temp = slots[i];
                slots[i] = slots[i + 1];
                slots[i + 1] = temp;
                PartyManager.instance.SaveParty();
                break;
            }
        }
    }
}
