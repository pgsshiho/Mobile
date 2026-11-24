using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/AlienBloated/BileSpew")]
public class BileSpew : SkillBase
{
    [Header("담즙 분사 설정 (고정 5 피해 + 산화 부여, 방어력 무시)")]
    [Tooltip("고정 피해량 (방어력 무시)")]
    public int fixedDamage = 5;

    [Tooltip("산화 지속 턴수")]
    public int oxidationTurns = 3;

    [Tooltip("산화 턴당 피해량")]
    public int oxidationDamage = 5;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 방어력 무시 고정 피해
        target.TakeDamage(fixedDamage);
        Debug.Log($"<color=green>[담즙 분사]</color> {user.Unitname}의 담즙으로 {target.Unitname}에게 방어 무시 고정 {fixedDamage} 피해!");

        // 산화 부여 (100% 확률)
        target.AddOxidation(oxidationTurns, oxidationDamage);
        Debug.Log($"{target.Unitname} 담즙 산화 부여 완료! ({oxidationTurns}턴, 턴당 {oxidationDamage} 피해)");
    }
}
