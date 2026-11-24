using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/GiantSpider/WebSpinning")]
public class WebSpinning : SkillBase
{
    [Header("Buff")]
    public BuffData webBuff;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (webBuff != null)
        {
            user.AddBuff(webBuff);
        }

        Debug.Log($"<color=green>[거미줄 치기]</color> {user.Unitname} 주변에 거미줄을 둘러 방어력과 기동성을 대폭 보강합니다!");
    }
}
