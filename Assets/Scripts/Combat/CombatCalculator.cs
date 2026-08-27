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
                    finalDefense += buff.data.defenseBonus;
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

    public static int CalculateDamage(Unit attacker, Unit target, SkillData skill)
    {
        if (attacker == null || skill == null) return 0;

        int damage = attacker.GetAttackPower() + skill.power;
        float multiplier = 1f;

        if (attacker.buffs != null)
        {
            foreach (BuffRuntime buff in attacker.buffs)
            {
                if (buff?.data != null)
                {
                    multiplier *= buff.data.damageMultiplier;
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
            Debug.Log($"<color=yellow>[크리티컬 발동!]</color> {attacker.Unitname} 데미지 2배 증폭 ({damage})");
        }

        damage = Mathf.Max(1, damage);

        // 표식 대상 추가 피해
        if (target != null && target.isMarked)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            Debug.Log($"{target.Unitname} 표식 추가 피해!");
        }

        // 대상이 금속 피로 상태일 때 파손/피격 피해 1.5배 증폭
        if (target != null && target.isMetalFatigue)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            Debug.Log($"[금속 피로] {target.Unitname} 구조적 결함으로 피해 증폭 ({damage})");
        }

        // 공격자의 녹 확률 판정 (공격 시 상대에게 녹/산화 부여)
        if (target != null && attacker.rustChance > 0)
        {
            if (Random.Range(0, 100) < attacker.rustChance)
            {
                // 상대 산화 저항력 체크
                if (Random.Range(0, 100) >= target.oxidationResist)
                {
                    target.AddStatus(StatusType.OxidationI, 3);
                    Debug.Log($"[녹 부여] {attacker.Unitname}의 공격으로 {target.Unitname} 산화 I 발생!");
                }
            }
        }

        return damage;
    }

    public static bool CheckHit(Unit attacker, Unit target, SkillData skill)
    {
        if (attacker == null || skill == null) return false;

        // 1. 대상 회피율 체크
        if (target != null)
        {
            float targetDodge = target.dodgeChance;

            // 윤활유 유출 상태이면 회피율 0 (피하지 못함)
            if (target.isLubricantLeak || target.isOilLeak)
            {
                targetDodge = 0f;
            }

            if (targetDodge > 0f && Random.value < targetDodge)
            {
                Debug.Log($"{target.Unitname} 공격 회피 성공!");

                // 잔상 현상: 적의 공격을 피할 때마다 명중률(정확도)이 -10씩 낮아짐
                if (target.isGhosting)
                {
                    target.ghostingMissPenalty += 10;
                    Debug.LogWarning($"[잔상 현상] {target.Unitname} 잔상 혼동으로 명중률 -10 누적 (총 페널티: -{target.ghostingMissPenalty})");
                }

                return false;
            }
        }

        // 2. 공격자 명중률 체크
        int finalHit = attacker.GetAccuracy() + skill.hitBonus;
        finalHit = Mathf.Clamp(finalHit, 0, 100);

        int hitRoll = Random.Range(0, 100);
        return hitRoll < finalHit;
    }
}
