using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Predator/SelfDestruct")]
public class SelfDestruct : SkillBase
{
    [Header("Pollution Settings")]
    [Range(0, 100)]
    public int pollutionChance = 60;
    public int pollutionTurns = 3;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (target != null)
        {
            int damage = user != null ? user.CalculateDamage(target, skill) : skill.power;
            target.TakeDamage(damage);

            // 오염 부여 확률
            int finalChance = Mathf.Clamp(pollutionChance - target.specialArmor, 0, 100);
            if (Random.Range(0, 100) < finalChance)
            {
                target.AddStatus(StatusType.WeaponPollution, pollutionTurns);
                Debug.Log($"{target.Unitname} 자폭 파편으로 무장 오염 부여!");
            }
        }

        // 자폭 시전자 사망 처리 (스킬 1회 시전 시 한 번만 죽도록)
        if (user != null && user.health > 0)
        {
            Debug.Log($"<color=red>[자폭]</color> {user.Unitname}이(가) 폭발하며 자폭했습니다!");
            user.health = 0;
            user.Die();
        }
    }
}
