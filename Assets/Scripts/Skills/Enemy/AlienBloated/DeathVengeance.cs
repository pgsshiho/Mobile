using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/AlienBloated/DeathVengeance")]
public class DeathVengeance : SkillBase
{
    [Header("죽음의 복수 설정 (사망 시 카운터 공격, 방어력 무시)")]
    [Tooltip("복수 추가 공격력")]
    public int vengeDamageBonus = 10;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;
        if (target.health <= 0) return;

        // 방어력 무시 복수 피해
        int damage = user.attackPower + vengeDamageBonus;
        target.TakeDamage(damage);
        Debug.Log($"<color=red>[죽음의 복수]</color> {user.Unitname}이(가) 죽으면서 {target.Unitname}에게 방어 무시 {damage} 복수 피해!");
    }
}
