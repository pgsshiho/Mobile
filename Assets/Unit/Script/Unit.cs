using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Stat")]
    public int speed;

    public float health;
    public float maxHealth;

    public int attackPower;
    public int defensePower;

    public float criticalChance;

    [Header("Accuracy")]
    public int accuracy = 90;

    [Header("Skill")]
    public List<SkillData> skills;

    public SkillData selectedSkill;

    [Header("Buff")]
    public List<BuffRuntime> buffs =
        new List<BuffRuntime>();

    [Header("Debuff")]
    public List<DebuffRuntime> debuffs =
        new List<DebuffRuntime>();

    [Header("State")]
    public bool isBleeding = false;
    public int bleedingCount = 0;

    public bool isStunned = false;

    public int fireCount = 0;
    public bool isFires = false;

    [Header("UI")]
    public GameObject myturnUI;
    public GameObject damageTextPrefab;
    public Transform damageTextSpawnPoint;

    [Header("Upgrade")]
    public int attackLevel = 0;
    public int defenseLevel = 0;

    public int maxAttackLevel = 10;
    public int maxDefenseLevel = 10;

    public bool accuracyupgrade = false;
    public string Unitname = "";

    // 턴 시작
    public virtual void MyTurn()
    {

        if (myturnUI != null)
        {
            myturnUI.SetActive(true);
        }

        BuffTurn();
        DebuffTurn();

        Bleeding();
        Fire();

        if (isStunned)
        {
            Debug.Log(name + " 기절");

            isStunned = false;

            TurnManager.instance.EndTurn();

            return;
        }

        if (health <= 0)
        {
            Die();
        }
    }

    // ==========================
    // 버프
    // ==========================

    void BuffTurn()
    {
        for (int i = buffs.Count - 1;
            i >= 0;
            i--)
        {
            buffs[i].turn--;

            if (buffs[i].turn <= 0)
            {
                Debug.Log(
                    Unitname +
                    " 의 " +
                    buffs[i].data.buffName +
                    " 종료"
                );

                buffs.RemoveAt(i);
            }
        }
    }

    public void AddBuff(BuffData buff)
    {
        BuffRuntime runtime =
            new BuffRuntime();

        runtime.data = buff;
        runtime.turn = buff.duration;

        buffs.Add(runtime);

        Debug.Log(
            Unitname +
            " 버프 추가 : " +
            buff.buffName
        );
    }

    public void ClearBuffs()
    {
        buffs.Clear();
    }

    // ==========================
    // 디버프
    // ==========================

    void DebuffTurn()
    {
        for (int i = debuffs.Count - 1;
            i >= 0;
            i--)
        {
            debuffs[i].turn--;

            if (debuffs[i].turn <= 0)
            {
                Debug.Log(
                    Unitname +
                    " 의 " +
                    debuffs[i].data.debuffName +
                    " 종료"
                );

                debuffs.RemoveAt(i);
            }
        }
    }

    public void AddDebuff(
        DebuffData debuff
    )
    {
        DebuffRuntime runtime =
            new DebuffRuntime();

        runtime.data = debuff;
        runtime.turn = debuff.duration;

        debuffs.Add(runtime);

        Debug.Log(
            Unitname +
            " 디버프 추가 : " +
            debuff.debuffName
        );
    }

    public void ClearDebuffs()
    {
        debuffs.Clear();
    }

    // ==========================
    // 상태이상
    // ==========================

    public void ClearStates()
    {
        isBleeding = false;
        bleedingCount = 0;

        isStunned = false;

        fireCount = 0;
        isFires = false;
    }

    void Bleeding()
    {
        if (!isBleeding)
            return;

        float damage =
            maxHealth * 0.05f;

        int finalDamage =
            Mathf.RoundToInt(damage);

        TakeDamage(
            finalDamage,
            DamageType.Bleed
        );

        Debug.Log(
            Unitname +
            " 출혈 피해 " +
            damage
        );

        bleedingCount++;

        if (bleedingCount >= 3)
        {
            isBleeding = false;

            bleedingCount = 0;

            Debug.Log(
                Unitname +
                " 출혈 종료"
            );
        }
    }

    void Fire()
    {
        if (fireCount == 1)
        {
            float damage =
                maxHealth * 0.04f;
            int finalDamage =
                Mathf.RoundToInt(damage);

            TakeDamage(
                finalDamage,
                DamageType.Fire
            );

            Debug.Log(
                Unitname +
                " 과열 피해 " +
                damage
            );

            fireCount++;
        }
        else if (fireCount == 2)
        {
            float damage =
                maxHealth * 0.08f;

            int finalDamage =
                Mathf.RoundToInt(damage);

            TakeDamage(
                finalDamage,
                DamageType.Fire
            );

            Debug.Log(
                Unitname +
                " 과열 피해 " +
                damage
            );

            fireCount++;
        }
        else if (fireCount >= 3)
        {
            isFires = true;
        }

        if (isFires)
        {
            float damage =
                maxHealth * 0.2f;

            int finalDamage =
                Mathf.RoundToInt(damage);

            TakeDamage(
                finalDamage,
                DamageType.Fire
            );

            Debug.Log(
                Unitname +
                " 화재 피해 " +
                damage
            );
        }
    }

    // ==========================
    // 스탯 계산
    // ==========================

    public int GetSpeed()
    {
        int finalSpeed = speed;

        foreach (BuffRuntime buff in buffs)
        {
            finalSpeed +=
                buff.data.speedBonus;
        }

        return finalSpeed;
    }

    public int GetAttackPower()
    {
        int finalAttack =
            attackPower;

        foreach (BuffRuntime buff in buffs)
        {
            finalAttack +=
                buff.data.attackBonus;
        }

        foreach (DebuffRuntime debuff in debuffs)
        {
            finalAttack -=
                debuff.data.attackPenalty;
        }

        return Mathf.Max(
            0,
            finalAttack
        );
    }

    public int GetDefensePower()
    {
        int finalDefense =
            defensePower;

        foreach (BuffRuntime buff in buffs)
        {
            finalDefense +=
                buff.data.defenseBonus;
        }

        foreach (DebuffRuntime debuff in debuffs)
        {
            finalDefense -=
                debuff.data.defensePenalty;
        }

        return Mathf.Max(
            0,
            finalDefense
        );
    }

    public float GetCriticalChance()
    {
        float finalCrit =
            criticalChance;

        foreach (BuffRuntime buff in buffs)
        {
            finalCrit +=
                buff.data.critBonus;
        }

        foreach (DebuffRuntime debuff in debuffs)
        {
            finalCrit -=
                debuff.data.critPenalty;
        }

        return Mathf.Max(
            0,
            finalCrit
        );
    }
    public float GetHealMultiplier()
    {
        float multiplier = 1f;

        foreach (BuffRuntime buff
            in buffs)
        {
            multiplier *=
                buff.data.healMultiplier;
        }

        return multiplier;
    }
    public void Heal(int amount)
    {
        health += amount;

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        Debug.Log(
            Unitname +
            " 회복 " +
            amount
        );
    }
    public int GetAccuracy()
    {
        int finalAccuracy =
            accuracy;

        foreach (BuffRuntime buff in buffs)
        {
            finalAccuracy +=
                buff.data.hitBonus;
        }

        foreach (DebuffRuntime debuff in debuffs)
        {
            finalAccuracy -=
                debuff.data.hitPenalty;
        }

        return Mathf.Clamp(
            finalAccuracy,
            0,
            100
        );
    }

    // ==========================
    // 전투 계산
    // ==========================

    public int CalculateDamage(
        Unit target,
        SkillData skill
    )
    {
        int damage =
            GetAttackPower() +
            skill.power;

        float multiplier = 1f;

        foreach (BuffRuntime buff
            in buffs)
        {
            multiplier *=
                buff.data.damageMultiplier;
        }

        damage =
            Mathf.RoundToInt(
                damage * multiplier
            );

        damage -=
            target.GetDefensePower();

        int roll =
            Random.Range(0, 100);

        if (roll <
            GetCriticalChance())
        {
            damage =
                Mathf.RoundToInt(
                    damage * 2f
                );

            Debug.Log(
                Unitname +
                " 크리티컬!"
            );
        }

        damage =
            Mathf.Max(
                1,
                damage
            );

        return damage;
    }

    public bool CheckHit(
        Unit target,
        SkillData skill
    )
    {
        int finalHit =
            GetAccuracy() +
            skill.hitBonus;

        finalHit =
            Mathf.Clamp(
                finalHit,
                0,
                100
            );

        int roll =
            Random.Range(0, 100);

        return roll < finalHit;
    }

    // ==========================
    // 기타
    // ==========================

    public virtual void SelectTarget(
        Unit target
    )
    {

    }

    Coroutine damageTextRoutine;
    public enum DamageType
    {
        Normal,
        Bleed,
        Fire,
        Heal
    }
    public virtual void TakeDamage(
        int damage,
        DamageType type = DamageType.Normal
    )
    {
        ShowDamageText(damage, type);

        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }
    void ShowDamageText(
    int damage,
    DamageType type
)
    {
        if (damageTextPrefab == null)
            return;

        Transform spawnPoint =
            damageTextSpawnPoint != null
            ? damageTextSpawnPoint
            : transform;

        GameObject obj =
            Instantiate(
                damageTextPrefab,
                spawnPoint.position,
                Quaternion.identity
            );

        TMP_Text text =
            obj.GetComponentInChildren<TMP_Text>();

        if (text != null)
        {
            text.text = damage.ToString();

            switch (type)
            {
                case DamageType.Bleed:
                    text.color = Color.red;
                    break;

                case DamageType.Fire:
                    text.color =
                        new Color(1f, 0.45f, 0f);
                    break;

                case DamageType.Heal:
                    text.color = Color.green;
                    text.text = "+" + damage;
                    break;

                default:
                    text.color = Color.white;
                    break;
            }
        }
    }

    public virtual void Die()
    {
        Debug.Log(Unitname + " 사망");

        TurnManager.instance
            .RemoveUnit(this);
        PartyManager.instance.Remove(this);
        if (myturnUI != null)
        {
            myturnUI.SetActive(false);
        }

        gameObject.SetActive(false);
    }
}