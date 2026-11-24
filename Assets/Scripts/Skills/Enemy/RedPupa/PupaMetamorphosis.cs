using UnityEngine;

[CreateAssetMenu(menuName = "SkillLogic/Enemy/RedPupa/PupaMetamorphosis")]
public class PupaMetamorphosis : SkillBase
{
    [Tooltip("변태까지 남은 최대 턴 수 (기획: 4턴)")]
    public int turnsRequired = 4;

    [Tooltip("변태 후 소환될 붉은나비 프리팹")]
    public GameObject butterflyPrefab;

    public override void Use(Unit user, Unit target, SkillData skill)
    {
        if (user == null || !(user is Enemy pupa)) return;

        pupa.pupaTurnCount++;
        int remaining = Mathf.Max(0, turnsRequired - pupa.pupaTurnCount);

        Debug.Log($"<color=red>[변태 진행중]</color> {pupa.Unitname} 고치가 요동칩니다! (남은 턴: {remaining}턴)");

        if (pupa.pupaTurnCount >= turnsRequired)
        {
            Debug.Log($"<color=purple>[변태 완료!]</color> {pupa.Unitname}이(가) 붉은 나비로 우화했습니다!");
            
            // 붉은나비 소환 및 번데기 제거
            GameObject prefabToSpawn = pupa.butterflyPrefab != null ? pupa.butterflyPrefab : butterflyPrefab;
            if (prefabToSpawn != null)
            {
                GameObject newButterfly = Object.Instantiate(prefabToSpawn, pupa.transform.position, pupa.transform.rotation);
                Enemy butterflyEnemy = newButterfly.GetComponent<Enemy>();
                if (butterflyEnemy != null && TurnManager.instance != null)
                {
                    TurnManager.instance.turnList.Add(butterflyEnemy);
                }
            }

            pupa.Die();
        }
    }
}
