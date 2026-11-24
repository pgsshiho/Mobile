using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/GiantSpider/SpiderCharge")]
public class SpiderCharge : SkillBase
{
    [Header("Main Target Damage")]
    public float directDamageMultiplier = 1.3f;

    [Header("Splash Stun Settings")]
    [Range(0, 100)]
    public int splashStunChance = 40;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 1. 메인 타겟 강타
        int mainDamage = Mathf.RoundToInt(user.CalculateDamage(target, skill) * directDamageMultiplier);
        target.TakeDamage(mainDamage);
        Debug.Log($"<color=orange>[거미 돌진]</color> {user.Unitname}의 육중한 몸체가 {target.Unitname}에게 돌진하여 {mainDamage} 피해!");

        // 2. 주변 아군(플레이어) 충격파 및 기절
        if (PartyManager.instance != null && PartyManager.instance.partySlots != null)
        {
            foreach (Unit party in PartyManager.instance.partySlots)
            {
                if (party == null || party == target || party.health <= 0) continue;

                int splashDmg = Mathf.Max(1, Mathf.RoundToInt(user.attackPower * 0.3f));
                party.TakeDamage(splashDmg);

                int finalChance = Mathf.Clamp(splashStunChance - party.specialArmor, 0, 100);
                if (Random.Range(0, 100) < finalChance)
                {
                    party.AddStatus(StatusType.Stun, 1);
                    Debug.Log($"<color=yellow>[충격파 기절]</color> {party.Unitname} 거미 돌진의 충격파로 기절!");
                }
            }
        }
    }
}
