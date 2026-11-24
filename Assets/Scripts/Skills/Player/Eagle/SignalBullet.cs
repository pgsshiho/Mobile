using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Player/Eagle/SignalBullet")]
public class SignalBullet : SkillBase
{
    [Header("Mark Settings & Duration")]
    [Range(0, 100)]
    [Tooltip("표식 부여 확률 (기본: 100%, 공식: (유닛 확률 - special ammo)%)")]
    public int markChance = 100;

    [Tooltip("표식 지속 턴")]
    public int markDuration = 2;

    [Header("Debuff")]
    public DebuffData signalDebuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        if (signalDebuff != null)
        {
            target.AddDebuff(signalDebuff);
        }

        // 공식: 상태이상 확률 = (유닛이 상태이상을 걸 숫자 - special ammo)%
        int finalChance = Mathf.Clamp(markChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            int appliedTurns = markDuration > 0 ? markDuration : (user != null ? user.markInflictTurns : 2);
            target.AddStatus(StatusType.Marked, appliedTurns);
            Debug.Log($"{target.Unitname} 신호 탄환 피격! 표식({appliedTurns}턴, 성공률: {finalChance}%) 부여 완료!");
        }
        else
        {
            Debug.Log($"{target.Unitname} 신호 탄환 표식 저항 성공! (기본확률 {markChance}% - special ammo {target.specialArmor}% = {finalChance}%)");
        }
    }
}
