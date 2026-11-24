using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/NavyCaptain/CallToArms")]
public class CallToArms : SkillBase
{
    [Tooltip("소환할 작은 외계인 프리팹 (Resources/Enemy/Parasite)")]
    public GameObject minionPrefab;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null) return;

        GameObject prefab = minionPrefab;
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Enemy/Parasite");
        }

        if (prefab == null)
        {
            Debug.LogWarning("[전원 집결] 소환할 외계인 프리팹을 찾지 못했습니다.");
            return;
        }

        int maxSlots = (BattleManager.instance != null && BattleManager.instance.Enemy != null && BattleManager.instance.Enemy.Length > 0)
            ? BattleManager.instance.Enemy.Length
            : 4;

        int spawnedCount = 0;
        for (int i = 0; i < 2; i++)
        {
            Enemy[] existingEnemies = Object.FindObjectsByType<Enemy>();
            int aliveCount = 0;
            foreach (Enemy e in existingEnemies)
            {
                if (e != null && e.health > 0) aliveCount++;
            }

            if (aliveCount >= maxSlots)
            {
                Debug.Log($"[전원 집결] 슬롯이 가득 차 더 이상 소환할 수 없습니다 ({aliveCount}/{maxSlots}).");
                break;
            }

            Vector3 spawnPos = user.transform.position + new Vector3(Random.Range(-1.5f, 1.5f), 0, Random.Range(-0.5f, 0.5f));
            if (BattleManager.instance != null && BattleManager.instance.Enemy != null)
            {
                for (int slotIdx = 0; slotIdx < BattleManager.instance.Enemy.Length; slotIdx++)
                {
                    Transform slot = BattleManager.instance.Enemy[slotIdx];
                    if (slot != null && slot.childCount == 0)
                    {
                        spawnPos = slot.position;
                        break;
                    }
                }
            }

            GameObject newMinion = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
            Enemy minionEnemy = newMinion.GetComponent<Enemy>();
            if (minionEnemy != null && TurnManager.instance != null)
            {
                TurnManager.instance.turnList.Add(minionEnemy);
            }
            spawnedCount++;
        }

        Debug.Log($"<color=orange>[전원 집결]</color> {user.Unitname}의 명령으로 작은 외계인 {spawnedCount}마리가 전장에 소환되었습니다!");
    }
}
