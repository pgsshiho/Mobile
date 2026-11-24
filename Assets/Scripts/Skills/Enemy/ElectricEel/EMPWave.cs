using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/ElectricEel/EMPWave")]
public class EMPWave : SkillBase
{
    [Header("EMP 파동 설정 (전체 플레이어 통신도 감소)")]
    [Tooltip("통신도 감소량")]
    public int commLoss = 20;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // target은 AllEnemy 루프에서 각 플레이어로 전달됨
        if (target.health <= 0) return;

        target.communication = Mathf.Clamp(target.communication - commLoss, -100, 100);
        Debug.Log($"<color=cyan>[EMP 파동]</color> {user.Unitname}의 EMP파동으로 {target.Unitname}의 통신도 -{commLoss}! (현재: {target.communication})");
    }
}
