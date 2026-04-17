using System.Collections.Generic;
using UnityEngine;

public interface ITargetingStrategy
{
    GameObject SelectTarget(List<GameObject> potentialTargets);
}

// 플레이어용: 클릭(터치)으로 선택
public class PlayerTargeting : ITargetingStrategy
{
    public GameObject SelectTarget(List<GameObject> potentialTargets)
    {
        // 실제 구현 시: 마우스 Raycast 클릭 로직을 여기에 작성
        Debug.Log("플레이어: 타겟을 터치하세요.");
        return null; // 터치 시 선택된 오브젝트 반환
    }
}

// 적군용: 랜덤 타겟 선택
public class RandomTargeting : ITargetingStrategy
{
    public GameObject SelectTarget(List<GameObject> potentialTargets)
    {
        if (potentialTargets == null || potentialTargets.Count == 0) return null;
        return potentialTargets[Random.Range(0, potentialTargets.Count)];
    }
}