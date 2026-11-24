using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 지네 (Centipede) 메인 유닛 클래스.
/// - 줄어들기 상태(1개체)와 늘어나기 상태(4개체)를 오가며 전투를 진행합니다.
/// - 늘어나기: 최대 HP 및 현재 HP를 4분할하여 4개체로 분열하며, 모든 상태이상을 4개체가 그대로 복제해갑니다.
/// - 줄어들기: 살아있는 개체들의 HP를 모두 합치고, 하나라도 걸린 상태이상이 있다면 그대로 가져와 1개체로 통합됩니다.
/// </summary>
public class CentipedeUnit : Enemy
{
    [Header("지네 상태 기믹")]
    [Tooltip("현재 늘어나기(4개체 분열) 상태 여부")]
    public bool isSplit = false;

    [Tooltip("줄어들기(합체) 상태 시의 본래 최대 체력")]
    public float originalMaxHp = 210f;

    [Tooltip("협공 스킬 발동 시 이번 턴 소모 여부")]
    public bool hasActedInCoordinatedAttack = false;

    [Header("스킬 데이터 바인딩")]
    public SkillData stretchSkill;       // 늘어나기 (줄어들기 상태 전용)
    public SkillData coilSkill;          // 휘어감기 (줄어들기 상태 전용)
    public SkillData shrinkSkill;        // 줄어들기 (늘어나기 상태 전용)
    public SkillData poisonSpraySkill;   // 독 뿌리기 (늘어나기 상태 전용)
    public SkillData pincerSkill;        // 협공 (늘어나기 상태 전용)
    public SkillData biteSkill;          // 물기 (늘어나기 상태 전용)

    [Header("분열 프리팹")]
    [Tooltip("늘어나기 시 소환될 지네 마디 프리팹 (Resources/Enemy/CentipedeSegment)")]
    public GameObject segmentPrefab;

    [Header("소환된 마디 목록")]
    public List<CentipedeSegmentUnit> spawnedSegments = new List<CentipedeSegmentUnit>();

    protected override void Awake()
    {
        base.Awake();
        if (originalMaxHp <= 0)
        {
            originalMaxHp = maxHealth > 0 ? maxHealth : 210f;
        }
    }

    protected override void Start()
    {
        base.Start();
        if (originalMaxHp <= 0)
        {
            originalMaxHp = maxHealth;
        }
    }

    public override void MyTurn()
    {
        // 협공으로 인해 이미 턴을 소모한 경우 이번 턴 자동 스킵
        if (hasActedInCoordinatedAttack)
        {
            hasActedInCoordinatedAttack = false;
            Debug.Log($"<color=orange>[협공 턴 소모]</color> {Unitname} 본체는 협공 참여로 이번 턴 행동을 마쳤습니다.");
            if (TurnManager.instance != null) TurnManager.instance.EndTurn();
            return;
        }

        base.MyTurn();
    }

    protected override SkillData ChooseSkill()
    {
        if (!isSplit)
        {
            // [줄어들기 상태] : 늘어나기 또는 휘어감기만 사용 가능
            List<SkillData> mergedSkills = new List<SkillData>();
            if (stretchSkill != null) mergedSkills.Add(stretchSkill);
            if (coilSkill != null) mergedSkills.Add(coilSkill);

            if (mergedSkills.Count > 0)
            {
                return mergedSkills[Random.Range(0, mergedSkills.Count)];
            }
        }
        else
        {
            // [늘어나기 상태] : 줄어들기, 독 뿌리기, 물기, 협공 사용 가능
            List<SkillData> splitSkills = new List<SkillData>();

            // 마디가 1개만 남아있거나 위기 상황이면 줄어들기 확률 상승
            int aliveCount = 1; // 본체
            foreach (var seg in spawnedSegments)
            {
                if (seg != null && seg.health > 0) aliveCount++;
            }

            if (aliveCount <= 2 && shrinkSkill != null && Random.value < 0.45f)
            {
                return shrinkSkill;
            }

            if (shrinkSkill != null) splitSkills.Add(shrinkSkill);
            if (poisonSpraySkill != null) splitSkills.Add(poisonSpraySkill);
            if (biteSkill != null) splitSkills.Add(biteSkill);
            if (pincerSkill != null && aliveCount >= 3) splitSkills.Add(pincerSkill);

            if (splitSkills.Count > 0)
            {
                return splitSkills[Random.Range(0, splitSkills.Count)];
            }
        }

        return base.ChooseSkill();
    }

    /// <summary>
    /// [늘어나기] 4칸으로 분열. 최대 체력 및 현재 체력을 4등분하고, 상태이상을 4개체 모두에게 복제합니다.
    /// </summary>
    public void SplitIntoFour()
    {
        if (isSplit) return;
        isSplit = true;

        float segmentMaxHp = Mathf.Max(1f, originalMaxHp / 4f);
        float segmentCurrentHp = Mathf.Max(1f, health / 4f);

        // 본체 체력 조정
        maxHealth = segmentMaxHp;
        health = segmentCurrentHp;
        UpdateHealthBar();

        // 현재 상태이상 목록 추출
        var currentStatuses = GetAllActiveStatuses();

        // 프리팹 로드
        GameObject prefabToSpawn = segmentPrefab;
        if (prefabToSpawn == null)
        {
            prefabToSpawn = Resources.Load<GameObject>("Enemy/CentipedeSegment");
        }
        if (prefabToSpawn == null)
        {
            prefabToSpawn = Resources.Load<GameObject>("Enemy/Centipede");
        }

        Transform[] enemySlots = BattleManager.instance != null ? BattleManager.instance.Enemy : null;

        // 본체는 0번 슬롯에 배치
        if (enemySlots != null && enemySlots.Length > 0 && enemySlots[0] != null)
        {
            transform.SetPositionAndRotation(enemySlots[0].position, enemySlots[0].rotation);
            BattleManager.instance.RegisterEnemy(this, 0);
        }

        spawnedSegments.Clear();

        // 3개의 마디 개체 생성 (총 4개체)
        for (int i = 1; i <= 3; i++)
        {
            Vector3 spawnPos = transform.position + new Vector3(i * 1.5f, 0, 0);
            Quaternion spawnRot = transform.rotation;

            if (enemySlots != null && i < enemySlots.Length && enemySlots[i] != null)
            {
                spawnPos = enemySlots[i].position;
                spawnRot = enemySlots[i].rotation;
            }

            GameObject segObj = Instantiate(prefabToSpawn, spawnPos, spawnRot);
            CentipedeSegmentUnit segment = segObj.GetComponent<CentipedeSegmentUnit>();
            if (segment == null)
            {
                segment = segObj.AddComponent<CentipedeSegmentUnit>();
            }

            segment.mainBody = this;
            segment.segmentIndex = i;
            segment.Unitname = $"지네 (마디 {i})";

            // 스탯 복제
            segment.originalMaxHp = originalMaxHp;
            segment.maxHealth = segmentMaxHp;
            segment.health = segmentCurrentHp;
            segment.attackPower = attackPower;
            segment.defensePower = defensePower;
            segment.accuracy = accuracy;
            segment.dodgeChance = dodgeChance;
            segment.speed = speed;
            segment.specialArmor = specialArmor;

            // 스킬 데이터 공유
            segment.stretchSkill = stretchSkill;
            segment.coilSkill = coilSkill;
            segment.shrinkSkill = shrinkSkill;
            segment.poisonSpraySkill = poisonSpraySkill;
            segment.pincerSkill = pincerSkill;
            segment.biteSkill = biteSkill;

            segment.skills.Clear();
            if (shrinkSkill != null) segment.skills.Add(shrinkSkill);
            if (poisonSpraySkill != null) segment.skills.Add(poisonSpraySkill);
            if (biteSkill != null) segment.skills.Add(biteSkill);
            if (pincerSkill != null) segment.skills.Add(pincerSkill);

            // 상태이상 복제 (4개체 모두 동일하게 상태이상을 부여받음)
            segment.CopyStatusesFrom(currentStatuses);

            // 등록
            if (BattleManager.instance != null)
            {
                BattleManager.instance.RegisterEnemy(segment, i);
            }
            if (TurnManager.instance != null && !TurnManager.instance.turnList.Contains(segment))
            {
                TurnManager.instance.turnList.Add(segment);
            }

            spawnedSegments.Add(segment);
        }

        if (BattleManager.instance != null)
        {
            BattleManager.instance.RearrangeEnemies();
        }

        Debug.Log($"<color=green><b>[늘어나기 발동]</b></color> 지네가 전장 4칸으로 늘어났습니다! 각 마디 체력: {segmentCurrentHp}/{segmentMaxHp}, 상태이상 {currentStatuses.Count}종 복제 완료.");
    }

    /// <summary>
    /// [줄어들기] 4개체에서 1개체로 통합. 모든 살아있는 개체의 체력을 합산하고,
    /// 하나라도 걸려있던 상태이상을 모두 모아 통합 개체가 이어받습니다.
    /// </summary>
    public void MergeIntoOne()
    {
        if (!isSplit) return;

        float totalCombinedHp = health;
        var pooledStatuses = GetAllActiveStatuses();

        // 소환된 마디들의 체력 및 상태이상 수집
        foreach (var seg in spawnedSegments)
        {
            if (seg != null && seg.health > 0)
            {
                totalCombinedHp += seg.health;

                var segStatuses = seg.GetAllActiveStatuses();
                foreach (var pair in segStatuses)
                {
                    if (pooledStatuses.TryGetValue(pair.Key, out var existing))
                    {
                        int maxTurns = existing.turns == -1 || pair.Value.turns == -1 ? -1 : Mathf.Max(existing.turns, pair.Value.turns);
                        int maxDmg = Mathf.Max(existing.dotDamage, pair.Value.dotDamage);
                        pooledStatuses[pair.Key] = (maxTurns, maxDmg);
                    }
                    else
                    {
                        pooledStatuses[pair.Key] = pair.Value;
                    }
                }
            }

            if (seg != null)
            {
                if (BattleManager.instance != null)
                {
                    BattleManager.instance.UnregisterEnemy(seg);
                }
                if (TurnManager.instance != null)
                {
                    TurnManager.instance.RemoveUnit(seg);
                }
                Destroy(seg.gameObject);
            }
        }

        spawnedSegments.Clear();

        // 본체 복원
        isSplit = false;
        maxHealth = originalMaxHp;
        health = Mathf.Min(maxHealth, totalCombinedHp);
        UpdateHealthBar();

        // 상태이상 일괄 적용
        ClearStates();
        CopyStatusesFrom(pooledStatuses);

        if (BattleManager.instance != null)
        {
            BattleManager.instance.RearrangeEnemies();
        }

        Debug.Log($"<color=cyan><b>[줄어들기 발동]</b></color> 지네가 1개체로 다시 합체했습니다! 통합 체력: {health}/{maxHealth}, 누적 상태이상 {pooledStatuses.Count}종 유지.");
    }

    /// <summary>
    /// 협공 발동 시: 다른 모든 살아있는 지네 마디들의 이번 턴 소모 처리
    /// </summary>
    public void TriggerCoordinatedTurnConsumption(Unit attacker)
    {
        if (attacker != this)
        {
            hasActedInCoordinatedAttack = true;
        }

        foreach (var seg in spawnedSegments)
        {
            if (seg != null && seg != attacker && seg.health > 0)
            {
                seg.hasActedInCoordinatedAttack = true;
            }
        }

        Debug.Log("<color=red>[협공 발동]</color> 모든 지네 마디가 일제히 공격에 참여하여 이번 라운드 턴을 소모했습니다!");
    }

    public override void Die()
    {
        // 만약 분열 상태에서 본체가 사망했다면, 살아있는 마디 중 하나를 새로운 메인 본체로 승격
        if (isSplit && spawnedSegments != null && spawnedSegments.Count > 0)
        {
            CentipedeSegmentUnit survivor = spawnedSegments.Find(s => s != null && s.health > 0);
            if (survivor != null)
            {
                spawnedSegments.Remove(survivor);
                survivor.PromoteToMainBody(this);
                base.Die();
                return;
            }
        }

        // 모든 마디 정리
        foreach (var seg in spawnedSegments)
        {
            if (seg != null && seg.health > 0)
            {
                seg.Die();
            }
        }
        spawnedSegments.Clear();

        base.Die();
    }
}
