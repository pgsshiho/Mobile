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
        if (unit.isOilLeak)
            finalSpeed -= 2;
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

        return Mathf.Max(0f, finalCrit);
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

        if (target != null)
        {
            damage -= target.GetDefensePower();
        }

        int roll = Random.Range(0, 100);
        if (roll < attacker.GetCriticalChance())
        {
            damage = Mathf.RoundToInt(damage * 2f);
            Debug.Log($"{attacker.Unitname} 크리티컬!");
        }

        damage = Mathf.Max(1, damage);

        if (target != null && target.isMarked)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            Debug.Log($"{target.Unitname} 표식 추가 피해!");
        }

        return damage;
    }

    public static bool CheckHit(Unit attacker, Unit target, SkillData skill)
    {
        if (attacker == null || skill == null) return false;

        int finalHit = attacker.GetAccuracy() + skill.hitBonus;
        finalHit = Mathf.Clamp(finalHit, 0, 100);

        int roll = Random.Range(0, 100);
        return roll < finalHit;
    }
}
