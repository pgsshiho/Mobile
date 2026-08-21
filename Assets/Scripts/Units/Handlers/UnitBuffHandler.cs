using System.Collections.Generic;
using UnityEngine;

public class UnitBuffHandler
{
    private readonly Unit owner;
    public List<BuffRuntime> Buffs { get; private set; }
    public List<DebuffRuntime> Debuffs { get; private set; }

    public UnitBuffHandler(Unit owner, List<BuffRuntime> buffsList, List<DebuffRuntime> debuffsList)
    {
        this.owner = owner;
        this.Buffs = buffsList ?? new List<BuffRuntime>();
        this.Debuffs = debuffsList ?? new List<DebuffRuntime>();
    }

    public void AddBuff(BuffData buff)
    {
        if (buff == null) return;

        BuffRuntime runtime = new BuffRuntime
        {
            data = buff,
            turn = buff.duration
        };

        Buffs.Add(runtime);
        Debug.Log($"{owner.Unitname} 버프 추가 : {buff.buffName}");
    }

    public void ClearBuffs()
    {
        Buffs.Clear();
    }

    public void BuffTurn()
    {
        for (int i = Buffs.Count - 1; i >= 0; i--)
        {
            Buffs[i].turn--;

            if (Buffs[i].turn <= 0)
            {
                Debug.Log($"{owner.Unitname} 의 {Buffs[i].data.buffName} 종료");
                Buffs.RemoveAt(i);
            }
        }
    }

    public void AddDebuff(DebuffData debuff)
    {
        if (debuff == null) return;

        DebuffRuntime runtime = new DebuffRuntime
        {
            data = debuff,
            turn = debuff.duration
        };

        Debuffs.Add(runtime);
        Debug.Log($"{owner.Unitname} 디버프 추가 : {debuff.debuffName}");
    }

    public void ClearDebuffs()
    {
        Debuffs.Clear();
    }

    public void DebuffTurn()
    {
        for (int i = Debuffs.Count - 1; i >= 0; i--)
        {
            Debuffs[i].turn--;

            if (Debuffs[i].turn <= 0)
            {
                Debug.Log($"{owner.Unitname} 의 {Debuffs[i].data.debuffName} 종료");
                Debuffs.RemoveAt(i);
            }
        }
    }
}
