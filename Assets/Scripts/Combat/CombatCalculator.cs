using UnityEngine;

public static class CombatCalculator
{
    public static int GetSpeed(Unit unit)
    {
        if (unit == null) return 0;

        int finalSpeed = unit.speed;

        if (unit.buffs != null)
        {
            foreach (BuffRuntime buff in unit.buffs)
            {
                if (buff?.data != null)
                {
                    finalSpeed += buff.data.speedBonus;
                }
            }
        }

        if (unit.debuffs != null)
        {
            foreach (DebuffRuntime debuff in unit.debuffs)
            {
                if (debuff?.data != null)
                {
                    finalSpeed -= debuff.data.speedPenalty;
                }
            }
        }

        if (unit.isOxidationI)
            finalSpeed -= 2;
        if (unit.isOxidationII)
            finalSpeed -= 4;
        if (unit.isLubricantLeak || unit.isOilLeak)
            finalSpeed -= 5;
        if (unit.isOilEmpty)
            finalSpeed -= 4;

        return Mathf.Max(0, finalSpeed);
    }

    public static int GetAttackPower(Unit unit)
    {
        if (unit == null) return 0;

        int finalAttack = unit.attackPower;

        if (unit.buffs != null)
        {
            foreach (BuffRuntime buff in unit.buffs)
            {
                if (buff?.data != null)
                {
                    finalAttack += buff.data.attackBonus;
                }
            }
        }

        if (unit.debuffs != null)
        {
            foreach (DebuffRuntime debuff in unit.debuffs)
            {
                if (debuff?.data != null)
                {
                    finalAttack -= debuff.data.attackPenalty;
                }
            }
        }

        return Mathf.Max(0, finalAttack);
    }

    public static int GetDefensePower(Unit unit)
    {
        if (unit == null) return 0;

        int finalDefense = unit.defensePower;

        if (unit.buffs != null)
        {
            foreach (BuffRuntime buff in unit.buffs)
            {
                if (buff?.data != null)
                {
                    finalDefense += Mathf.RoundToInt(buff.data.defenseBonus);
                }
            }
        }

        if (unit.debuffs != null)
        {
            foreach (DebuffRuntime debuff in unit.debuffs)
            {
                if (debuff?.data != null)
                {
                    finalDefense -= debuff.data.defensePenalty;
                }
            }
        }

        // 금속 피로: 장갑(물리 방어력) 감소
        if (unit.isMetalFatigue)
        {
            finalDefense -= 4;
        }

        return Mathf.Max(0, finalDefense);
    }

    public static float GetCriticalChance(Unit unit)
    {
        if (unit == null) return 0f;

        float finalCrit = unit.criticalChance;

        if (unit.buffs != null)
        {
            foreach (BuffRuntime buff in unit.buffs)
            {
                if (buff?.data != null)
                {
                    finalCrit += buff.data.critBonus;
                }
            }
        }

        if (unit.debuffs != null)
        {
            foreach (DebuffRuntime debuff in unit.debuffs)
            {
                if (debuff?.data != null)
                {
                    finalCrit -= debuff.data.critPenalty;
                }
            }
        }

        return Mathf.Clamp(finalCrit, 0f, 100f);
    }

    public static float GetHealMultiplier(Unit unit)
    {
        if (unit == null) return 1f;

        float multiplier = 1f;

        if (unit.buffs != null)
        {
            foreach (BuffRuntime buff in unit.buffs)
            {
                if (buff?.data != null)
                {
                    multiplier *= buff.data.healMultiplier;
                }
            }
        }

        return multiplier;
    }

    public static int GetAccuracy(Unit unit)
    {
        if (unit == null) return 0;

        int finalAccuracy = unit.accuracy;

        if (unit.buffs != null)
        {
            foreach (BuffRuntime buff in unit.buffs)
            {
                if (buff?.data != null)
                {
                    finalAccuracy += buff.data.hitBonus;
                }
            }
        }

        if (unit.debuffs != null)
        {
            foreach (DebuffRuntime debuff in unit.debuffs)
            {
                if (debuff?.data != null)
                {
                    finalAccuracy -= debuff.data.hitPenalty;
                }
            }
        }

        // 잔상 현상: 공격 회피 시마다 누적된 명중률 페널티 적용
        if (unit.isGhosting)
        {
            finalAccuracy -= unit.ghostingMissPenalty;
        }

        return Mathf.Clamp(finalAccuracy, 0, 100);
    }

    /// <summary>
    /// 버프/디버프/상태이상을 모두 반영한 최종 회피율을 계산합니다.
    ///  - dodgeBonus: 고정 수치 추가
    ///  - dodgeMultiplier: 배율 적용 (곱연산)
    ///  - 오일 누출/윤활유 누출 상태이면 0으로 강제 처리
    /// </summary>
    public static float GetDodgeChance(Unit unit)
    {
        if (unit == null) return 0f;

        // 오일 누출 / 윤활유 누출: 회피 불가
        if (unit.isLubricantLeak || unit.isOilLeak)
            return 0f;

        float baseDodge = unit.dodgeChance;
        float bonusAdd  = 0f;
        float multiplier = 1f;

        // 버프: 고정 추가 & 배율
        if (unit.buffs != null)
        {
            foreach (BuffRuntime buff in unit.buffs)
            {
                if (buff?.data != null)
                {
                    bonusAdd  += buff.data.dodgeBonus;
                    multiplier *= buff.data.dodgeMultiplier;
                }
            }
        }

        // 디버프: 고정 감소 & 배율 패널티
        if (unit.debuffs != null)
        {
            foreach (DebuffRuntime debuff in unit.debuffs)
            {
                if (debuff?.data != null)
                {
                    bonusAdd  -= debuff.data.dodgePenalty;
                    multiplier *= debuff.data.dodgeMultiplierPenalty;
                }
            }
        }

        // 고정 수치를 먼저 더한 뒤 배율 적용, 0~1 사이로 클램프
        return Mathf.Clamp01((baseDodge + bonusAdd) * multiplier);
    }

    public static int CalculateDamage(Unit attacker, Unit target, SkillData skill)
    {
        if (attacker == null || skill == null) return 0;

        int damage = attacker.GetAttackPower() + skill.power;

        // 데미지 배수 합연산
        // 기본 배율 1.0에서 각 버프의 증가·감소분을 더한다.
        float multiplier = 1f;

        if (attacker.buffs != null)
        {
            foreach (BuffRuntime buff in attacker.buffs)
            {
                if (buff?.data != null)
                {
                    multiplier += buff.data.damageMultiplier - 1f;
                }
            }
        }

        damage = Mathf.RoundToInt(damage * multiplier);

        // 장갑(방어력) 차감
        if (target != null)
        {
            damage -= target.GetDefensePower();
        }

        // 크리티컬 발동: 데미지 2배
        int roll = Random.Range(0, 100);

        if (roll < attacker.GetCriticalChance())
        {
            damage = Mathf.RoundToInt(damage * 2f);
            // 빗나감 표기와 동일하게, 크리티컬도 피격 대상 위에 표시한다.
            target?.ShowCombatText(LocalizationManager.GetLocalizedString("Chri"), new Color(1f, 0.82f, 0.1f));
            Debug.Log($"<color=yellow>[크리티컬 발동!]</color> {attacker.Unitname} 데미지 2배 증폭 ({damage})");
        }

        damage = Mathf.Max(1, damage);

        // 표식 대상 추가 피해
        if (target != null && target.isMarked)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            Debug.Log($"{target.Unitname} 표식 추가 피해!");
        }

        // 대상이 금속 피로 상태일 때 피해 1.5배 증폭
        if (target != null && target.isMetalFatigue)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            Debug.Log($"[금속 피로] {target.Unitname} 구조적 결함으로 피해 증폭 ({damage})");
        }

        // 공격자의 녹 확률 판정 (공식: 상태이상 확률 = 유닛 확률 - special ammo / specialArmor)
        if (target != null && attacker.rustChance > 0)
        {
            int finalRustChance = CalculateStatusChance(attacker.rustChance, target);
            if (Random.Range(0, 100) < finalRustChance)
            {
                int turns = attacker.oxidationTurns > 0 ? attacker.oxidationTurns : 3;
                int dotDmg = attacker.oxidationDamage > 0 ? attacker.oxidationDamage : attacker.rustDamage;
                target.AddOxidation(turns, dotDmg);
                Debug.Log($"[녹 부여] {attacker.Unitname}의 공격으로 {target.Unitname} 산화 부여! (최종확률: {finalRustChance}%, 지속: {turns}턴, 턴당 피해: {dotDmg})");
            }
        }

        return damage;
    }

    public static bool CheckHit(Unit attacker, Unit target, SkillData skill)
    {
        if (attacker == null || skill == null)
            return false;

        // 1. 대상 회피율 체크 (버프/디버프/상태이상 모두 반영)
        if (target != null)
        {
            float targetDodge = GetDodgeChance(target);

            if (targetDodge > 0f && Random.value < targetDodge)
            {
                Debug.Log($"{target.Unitname} 공격 회피 성공!");

                // 회피한 대상에게 메시지 출력
                target.ShowCombatText(LocalizationManager.GetLocalizedString("Avoid"), Color.gray);

                if (target.isGhosting)
                {
                    target.ghostingMissPenalty += 10;

                    Debug.LogWarning(
                        $"[잔상 현상] {target.Unitname} 잔상 혼동으로 명중률 -10 누적 " +
                        $"(총 페널티: -{target.ghostingMissPenalty})"
                    );
                }

                return false;
            }
        }

        // 2. 공격자 명중률 체크
        int finalHit = attacker.GetAccuracy() + skill.hitBonus;
        finalHit = Mathf.Clamp(finalHit, 0, 100);

        int hitRoll = Random.Range(0, 100);

        if (hitRoll >= finalHit)
        {
            // 명중에 실패했을 때 공격 대상에게 표시
            if (target != null)
            {
                target.ShowCombatText(LocalizationManager.GetLocalizedString("Avoid"), Color.gray);
            }

            return false;
        }

        return true;
    }
    /// <summary>
    /// 대상의 특수장갑(specialArmor / special ammo)을 감안한 최종 상태이상 부여 확률을 계산합니다.
    /// 공식: 최종 상태이상 확률 = 유닛(스킬) 확률 - 대상의 specialArmor
    /// </summary>
    public static int CalculateStatusChance(int baseChance, Unit target)
    {
        if (target == null) return Mathf.Clamp(baseChance, 0, 100);
        int resist = target.specialArmor;
        return Mathf.Clamp(baseChance - resist, 0, 100);
    }

    /// <summary>
    /// 상태이상 확률 판정을 수행합니다. (특수장갑 감쇄 반영)
    /// </summary>
    public static bool CheckStatusChance(int baseChance, Unit target)
    {
        int finalChance = CalculateStatusChance(baseChance, target);
        return Random.Range(0, 100) < finalChance;
    }

}
