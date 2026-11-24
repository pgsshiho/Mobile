using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Mole/Spreadingsoil")]
public class Spreadingsoil : SkillBase
{
    public DebuffData blindDebuff;

    [Header("Bleed Settings")]
    [Range(0, 100)]
    [Tooltip("실명 부여 확률 (기획: 90%, 대상의 specialArmor/special ammo에 의해 감쇄)")]
    public int BlindChance = 90;

    [Tooltip("실명 지속 턴")]
    public int blindDuration = 3;

    public int blindAmount = 20; // 실명 상태이상 턴당 피해량은 0으로 설정
    public override void Use(
    Unit user,
    Unit target,
    SkillData skill
)
    {
        // 명중 체크
        if (!user.CheckHit(target, skill))
        {
            Debug.Log("빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);

        // 공식: 상태이상 확률 = 유닛(스킬) 확률 - special ammo(specialArmor)
        int finalChance = Mathf.Clamp(BlindChance - target.specialArmor, 0, 100);
        if (Random.Range(0, 100) < finalChance)
        {
            target.AddDebuff(blindDebuff);
        }
        else
        {
            Debug.Log($"{target.name}이(가) 실명 저항 성공! (실명 확률 {BlindChance}% - 특수장갑 {target.specialArmor}% = {finalChance}%)");

        }
    }
}
