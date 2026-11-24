using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/ElectricEel/SlimeSec")]
public class SlimeSec : SkillBase
{
    [Header("점액 분비 설정 (회피율 +15% 버프, 3턴)")]
    [Tooltip("점액 회피 버프 데이터 (BuffData에서 dodgeChance +0.15, 3턴 설정)")]
    public BuffData slimeBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (slimeBuff != null)
        {
            user.AddBuff(slimeBuff);
            Debug.Log($"<color=green>[점액 분비]</color> {user.Unitname} 점액 분비로 회피율 상승 버프 획득!");
        }
        else
        {
            Debug.LogWarning($"[점액 분비] {user.Unitname}: slimeBuff가 할당되지 않았습니다. Inspector에서 BuffData를 연결해주세요.");
        }
    }
}
