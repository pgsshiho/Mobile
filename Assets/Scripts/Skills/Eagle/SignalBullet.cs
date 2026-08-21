using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Eagle/SignalBullet")]
public class SignalBullet : SkillBase
{
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

        // 표식 및 방어 감소
        target.AddStatus(StatusType.Marked, 2);
        Debug.Log($"{target.Unitname} 신호 탄환 피격! 회피 및 방어력 무력화!");
    }
}
