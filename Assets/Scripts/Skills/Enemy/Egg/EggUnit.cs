using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 알 전용 Enemy 클래스.
/// 2턴 후 올챙이로 부화합니다.
/// </summary>
public class EggUnit : Enemy
{
    [Header("부화 설정")]
    [Tooltip("부화에 필요한 턴 수")]
    public int turnsToHatch = 2;
    public int currentTurnCount = 0;

    [Tooltip("부화할 올챙이 프리팹 (Resources/Enemy/Tadpole)")]
    public GameObject tadpolePrefab;

    public override void MyTurn()
    {
        currentTurnCount++;
        Debug.Log($"<color=yellow>[알 부화 대기]</color> {Unitname} 껍질 속에서 무언가 움직입니다... ({currentTurnCount}/{turnsToHatch}턴)");

        if (currentTurnCount >= turnsToHatch)
        {
            Hatch();
            if (TurnManager.instance != null) TurnManager.instance.EndTurn();
            return;
        }

        base.MyTurn();
    }

    private void Hatch()
    {
        Debug.Log($"<color=green>[부화 완료!]</color> {Unitname}이(가) 깨어나 올챙이가 나타났습니다!");

        GameObject prefab = tadpolePrefab;
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Enemy/Tadpole");
        }

        if (prefab != null)
        {
            GameObject newTadpole = Instantiate(prefab, transform.position, transform.rotation);
            Enemy tadpoleEnemy = newTadpole.GetComponent<Enemy>();
            if (tadpoleEnemy != null && TurnManager.instance != null)
            {
                TurnManager.instance.turnList.Add(tadpoleEnemy);
            }
        }

        Die();
    }
}
