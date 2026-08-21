using UnityEngine;

public abstract class SkillBase : ScriptableObject
{
    public abstract void Use(
        Unit user,
        Unit target,
        SkillData skill
    );
}