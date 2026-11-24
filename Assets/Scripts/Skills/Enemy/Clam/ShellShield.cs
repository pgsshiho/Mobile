using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Clam/ShellShield")]
public class ShellShield : SkillBase
{
    [Header("껍데기 방패 설정 (방어력 +20 버프 + 도발, 3턴)")]
    [Tooltip("방어 버프 데이터 (BuffData에서 defensePower +20, 3턴 설정)")]
    public BuffData shellBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (shellBuff != null)
        {
            user.AddBuff(shellBuff);
            Debug.Log($"<color=blue>[껍데기 방패]</color> {user.Unitname} 껍데기 방패로 방어력 강화 및 도발 발동!");
        }
        else
        {
            Debug.LogWarning($"[껍데기 방패] {user.Unitname}: shellBuff가 할당되지 않았습니다. Inspector에서 BuffData를 연결해주세요.");
        }
    }
}
