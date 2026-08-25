using System;

[Serializable]
public class ItemRuntime
{
    public ItemData data;
    public int count;        // 현재 보유 수량
    public int usedCount;    // 사용 횟수 (한도 체크용)

}
