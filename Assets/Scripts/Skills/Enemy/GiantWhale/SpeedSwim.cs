using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/GiantWhale/SpeedSwim")]
public class SpeedSwim : SkillBase
{
    [Header("버프 설정")]
    [Tooltip("속도 증가치")]
    public int speedBonus = 10;

    [Tooltip("연결할 BuffData (속도 +10)")]
    public BuffData speedBuff;

    private int lastFrameApplied = -1;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 속도 증가는 프레임당 1회만 적용 (AllEnemy 루프로 여러 번 호출되는 경우 방지)
        int currentFrame = Time.frameCount;
        if (lastFrameApplied != currentFrame)
        {
            lastFrameApplied = currentFrame;
            if (speedBuff != null)
            {
                user.AddBuff(speedBuff);
            }
            else
            {
                user.speed += speedBonus;
                Debug.Log($"<color=blue>[헤엄치기]</color> {user.Unitname}의 속도가 {speedBonus} 증가했습니다! (현재 속도: {user.speed})");
            }
        }

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 헤엄치기 파도가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=cyan>[헤엄치기]</color> {user.Unitname}의 헤엄치기 충격파로 {target.Unitname}에게 {damage} 피해!");
    }
}
