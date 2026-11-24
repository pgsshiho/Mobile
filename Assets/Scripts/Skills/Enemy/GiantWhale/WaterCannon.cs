using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/GiantWhale/WaterCannon")]
public class WaterCannon : SkillBase
{
    [Header("산화 설정")]
    [Range(0, 100)]
    public int oxidationChance = 60;
    public int oxidationTurns = 3;
    public int oxidationDamage = 8;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 물대포가 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=cyan>[물대포]</color> {user.Unitname}이(가) {target.Unitname}에게 물대포 발사! {damage} 피해!");

        // 맨 뒤로 밀어내기
        if (PartyManager.instance != null && target is PlayerUnit player)
        {
            int currentIdx = PartyManager.instance.GetPlayerIndex(player);
            int lastIdx = PartyManager.instance.partySlots.Length - 1;
            if (currentIdx >= 0 && currentIdx < lastIdx)
            {
                PartyManager.instance.SwapPartySlots(currentIdx, lastIdx);
                Debug.Log($"<color=orange>[진형 붕괴]</color> {target.Unitname}이(가) 맨 뒤(4열)로 밀려났습니다!");
            }
        }

        // 산화 부여
        user.InflictStatusWithChance(target, StatusType.OxidationI, oxidationChance, oxidationTurns, oxidationDamage);
    }
}
