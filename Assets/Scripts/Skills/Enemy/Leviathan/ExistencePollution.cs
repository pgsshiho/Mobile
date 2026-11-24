using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Leviathan/ExistencePollution")]
public class ExistencePollution : SkillBase
{
    [Header("상태이상 부여 설정")]
    [Range(0, 100)]
    public int baseChance = 80;
    public int duration = 3;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 합선(CircuitryShort/ShortCircuit) 또는 오염(WeaponPollution) 무작위 결정
        StatusType chosenStatus = Random.value < 0.5f ? StatusType.ShortCircuit : StatusType.WeaponPollution;

        bool inflicted = user.InflictStatusWithChance(target, chosenStatus, baseChance, duration, 0);
        if (inflicted)
        {
            Debug.Log($"<color=purple>[존재 오염]</color> {target.Unitname}에게 [{chosenStatus}] 부여 성공!");
        }
    }
}
