[System.Serializable]
public class StatusEffect
{
    public string effectName;
    public int duration; // 남은 턴
    public System.Action<UnitBase> onTick; // 매 턴 실행될 효과
    public System.Action<UnitBase> onRemoved; // 해제 시 실행될 효과

    public StatusEffect(string name, int turns, System.Action<UnitBase> tick = null, System.Action<UnitBase> removed = null)
    {
        effectName = name;
        duration = turns;
        onTick = tick;
        onRemoved = removed;
    }
}