using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Centipede/CentipedePincer")]
public class CentipedePincer : SkillBase
{
    private int lastFrameTriggered = -1;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || target == null) return;

        // 전체 공격이므로 첫 번째 타겟 적중 시 다른 마디들의 턴 소모를 1회만 트리거 (프레임 기반 중복 방지)
        int currentFrame = Time.frameCount;
        if (lastFrameTriggered != currentFrame)
        {
            lastFrameTriggered = currentFrame;

            if (user is CentipedeUnit mainCentipede)
            {
                mainCentipede.TriggerCoordinatedTurnConsumption(user);
            }
            else if (user is CentipedeSegmentUnit segment && segment.mainBody != null)
            {
                segment.mainBody.TriggerCoordinatedTurnConsumption(user);
            }
        }

        if (!user.CheckHit(target, skill))
        {
            Debug.Log($"{user.Unitname}의 협공이 {target.Unitname}에게 빗나감!");
            return;
        }

        int damage = user.CalculateDamage(target, skill);
        target.TakeDamage(damage);
        Debug.Log($"<color=red><b>[협공]</b></color> 지네 마디들의 일제 협공으로 {target.Unitname}에게 {damage} 피해!");
    }
}
