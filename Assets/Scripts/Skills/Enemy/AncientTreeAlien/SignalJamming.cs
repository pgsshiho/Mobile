using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/AncientTreeAlien/SignalJamming")]
public class SignalJamming : SkillBase
{
    [Header("Communication Penalty")]
    [Tooltip("감소시킬 통신도 수치")]
    public int communicationLoss = 20;

    [Header("Debuff")]
    public DebuffData jamDebuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (target == null) return;

        // 플레이어 통신도 직접 감소
        target.communication = Mathf.Clamp(target.communication - communicationLoss, -100, 100);
        Debug.Log($"<color=magenta>[전파 차단]</color> 외계 노이즈 파동으로 {target.Unitname}의 통신도가 {communicationLoss} 하락했습니다! (현재: {target.communication})");

        if (jamDebuff != null)
        {
            target.AddDebuff(jamDebuff);
        }
    }
}
