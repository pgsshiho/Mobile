using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/NavyCaptain/Reload")]
public class Reload : SkillBase
{
    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        if (user is NavyCaptainUnit captain)
        {
            captain.needsReload = false;
        }

        Debug.Log($"<color=yellow>[재장전]</color> {user.Unitname}이(가) 대포에 새로운 포탄을 장전했습니다! (다음 대포발사 준비 완료)");
    }
}
