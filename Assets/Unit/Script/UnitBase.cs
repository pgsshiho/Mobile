using UnityEngine;
using System.Collections.Generic;

public abstract class UnitBase : MonoBehaviour, ITakeDamage
{
    [Header("Position")]
    public int gridPosition; // 1~4열 (1이 전열, 4가 후열)
    public bool isPlayerSide; // 플레이어 진영인지 적 진영인지

    [Header("Stats")]
    public float maxHp, currentHp, damage, speed, evasionRate, accuracy;

    // 상태 이상 리스트
    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    public virtual void TurnStart()
    {
        // 지속 시간 관리 및 효과 적용
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            activeEffects[i].onTick?.Invoke(this);
            activeEffects[i].duration--;
            if (activeEffects[i].duration <= 0)
            {
                activeEffects[i].onRemoved?.Invoke(this);
                activeEffects.RemoveAt(i);
            }
        }
    }

    public abstract void ExecuteTurn(System.Action onComplete); // 행동 종료 후 콜백

    public virtual void TakeDamage(float dmg, float acc, float p, float pd) { /* 기존 로직 */ }
    public void Die() { Destroy(gameObject); }
}