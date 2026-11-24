using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/Frog/LayEgg")]
public class LayEgg : SkillBase
{
    [Tooltip("소환할 알 프리팹 (Resources/Enemy/Egg 또는 직접 할당)")]
    public GameObject eggPrefab;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        // 적 최대 인원수(4마리) 체크
        Enemy[] existingEnemies = Object.FindObjectsByType<Enemy>();
        int aliveCount = 0;
        foreach (Enemy e in existingEnemies)
        {
            if (e != null && e.health > 0) aliveCount++;
        }

        int maxSlots = (BattleManager.instance != null && BattleManager.instance.Enemy != null && BattleManager.instance.Enemy.Length > 0)
            ? BattleManager.instance.Enemy.Length
            : 4;

        if (aliveCount >= maxSlots)
        {
            Debug.Log($"<color=yellow>[알 낳기 실패]</color> 적 슬롯이 가득 찼습니다 ({aliveCount}/{maxSlots})!");
            return;
        }

        GameObject prefabToSpawn = eggPrefab;
        if (prefabToSpawn == null)
        {
            prefabToSpawn = Resources.Load<GameObject>("Enemy/Egg");
        }

        if (prefabToSpawn != null)
        {
            Vector3 spawnPos = user.transform.position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-0.5f, 0.5f));
            if (BattleManager.instance != null && BattleManager.instance.Enemy != null)
            {
                // 빈 슬롯 위치 탐색
                for (int i = 0; i < BattleManager.instance.Enemy.Length; i++)
                {
                    Transform slot = BattleManager.instance.Enemy[i];
                    if (slot != null && slot.childCount == 0)
                    {
                        spawnPos = slot.position;
                        break;
                    }
                }
            }

            GameObject spawnedEgg = Object.Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            Enemy eggEnemy = spawnedEgg.GetComponent<Enemy>();
            if (eggEnemy != null && TurnManager.instance != null)
            {
                TurnManager.instance.turnList.Add(eggEnemy);
            }

            Debug.Log($"<color=green>[알 낳기]</color> {user.Unitname}이(가) 새로운 알을 낳았습니다!");
        }
        else
        {
            Debug.LogWarning("[알 낳기] 소환할 알 프리팹을 찾을 수 없습니다.");
        }
    }
}
