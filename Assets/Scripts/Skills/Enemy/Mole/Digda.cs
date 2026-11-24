using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Mole/Digda")]

public class Digda : SkillBase
{
    public BuffData DigdaBuff;
    [Tooltip("땅에숨기 지속 턴 (기획: 2턴)")]
    public int DIgdaDuration = 1;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (DigdaBuff != null)
        {
            user.AddBuff(DigdaBuff);
        }
    }
}
