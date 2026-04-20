using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    // 아군 파티 (최대 4명)
    // List의 0번 인덱스가 1열, 3번 인덱스가 4열이라고 가정
    public List<Unit> playerParty = new List<Unit>();
    public List<Unit> enemyParty = new List<Unit>();

    void UpdateRanks()
    {
        // 아군 위치 갱신
        for (int i = 0; i < playerParty.Count; i++)
        {
            playerParty[i].CurrentRank = i + 1; // 인덱스 0은 1열, 1은 2열...
        }

        // 적군 위치 갱신
        for (int i = 0; i < enemyParty.Count; i++)
        {
            enemyParty[i].CurrentRank = i + 1;
        }
    }

    // 캐릭터가 죽었을 때 대열 정비
    public void OnUnitDie(Unit deadUnit)
    {
        if (playerParty.Contains(deadUnit))
        {
            playerParty.Remove(deadUnit);
        }
        else if (enemyParty.Contains(deadUnit))
        {
            enemyParty.Remove(deadUnit);
        }

        UpdateRanks(); // 죽은 자리를 메우기 위해 위치 재계산
    }
}