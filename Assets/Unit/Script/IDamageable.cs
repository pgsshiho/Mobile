public interface IDamageable
{
    // 공격을 받았을 때 실행되는 메인 메서드
    void TakeDamage(float amount, bool isCrit, Unit attacker);

    // 지속 데미지(중독, 출혈) 처리를 위한 메서드
    void TakeDotDamage(float amount, OverTimeEffect.EffectType dotType);

    // 회복 처리
    void Heal(float amount);
}