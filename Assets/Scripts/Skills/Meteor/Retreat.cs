using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Meteor/Retreat")]
public class Retreat : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (PartyManager.instance != null)
        {
            MoveToBack(user);
        }

        Debug.Log($"{user.Unitname} 최후방으로 후퇴 완료!");
    }

    private void MoveToBack(Unit user)
    {
        var slots = PartyManager.instance.partySlots;
        if (slots == null) return;

        int currentIndex = -1;
        int lastOccupiedIndex = -1;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == user)
            {
                currentIndex = i;
            }
            if (slots[i] != null)
            {
                lastOccupiedIndex = i;
            }
        }

        if (currentIndex == -1 || lastOccupiedIndex <= currentIndex)
            return;

        // 유닛들을 한 칸씩 앞으로 밀고 user를 최후방 점유 슬롯으로 이동
        Unit movingUnit = slots[currentIndex];
        for (int i = currentIndex; i < lastOccupiedIndex; i++)
        {
            slots[i] = slots[i + 1];
        }
        slots[lastOccupiedIndex] = movingUnit;

        PartyManager.instance.SaveParty();
    }
}
