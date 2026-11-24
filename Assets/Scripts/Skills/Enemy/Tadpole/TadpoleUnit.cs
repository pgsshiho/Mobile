using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 올챙이 전용 Enemy 클래스.
/// 3턴 후 개구리로 성장합니다.
/// </summary>
public class TadpoleUnit : Enemy
{
    [Header("성장 설정")]
    [Tooltip("성장에 필요한 턴 수")]
    public int turnsToGrow = 3;
    public int currentTurnCount = 0;

    [Tooltip("성장할 개구리 프리팹 (Resources/Enemy/Frog)")]
    public GameObject frogPrefab;

    public override void MyTurn()
    {
        currentTurnCount++;
        Debug.Log($"<color=green>[올챙이 성장 중]</color> {Unitname} 다리가 돋아나고 있습니다... ({currentTurnCount}/{turnsToGrow}턴)");

        if (currentTurnCount >= turnsToGrow)
        {
            Metamorphose();
            if (TurnManager.instance != null) TurnManager.instance.EndTurn();
            return;
        }

        base.MyTurn();
    }

    private void Metamorphose()
    {
        Debug.Log($"<color=green>[성장 완료!]</color> {Unitname}이(가) 거대한 개구리로 성장했습니다!");

        GameObject prefab = frogPrefab;
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Enemy/Frog");
        }

        if (prefab != null)
        {
            GameObject newFrog = Instantiate(prefab, transform.position, transform.rotation);
            Enemy frogEnemy = newFrog.GetComponent<Enemy>();
            if (frogEnemy != null && TurnManager.instance != null)
            {
                TurnManager.instance.turnList.Add(frogEnemy);
            }
        }

        Die();
    }
}
