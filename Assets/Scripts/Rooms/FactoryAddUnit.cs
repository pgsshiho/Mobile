using System;
using UnityEngine;

public class FactoryAddUnit : MonoBehaviour
{
    public Unit MyUnit;
    public bool CanClick = false;

    // ── 정적 Action: 씬의 모든 FactoryAddUnit 인스턴스에게 브로드캐스트 ──
    public static event Action OnUnlockAll;

    private void OnEnable()
    {
        OnUnlockAll += Unlock;
    }

    private void OnDisable()
    {
        OnUnlockAll -= Unlock;
    }

    /// <summary>
    /// OnUnlockAll 이벤트를 받아 CanClick을 true로 갱신합니다.
    /// </summary>
    private void Unlock()
    {
        CanClick = true;
    }

    /// <summary>
    /// 씬 내 모든 FactoryAddUnit의 CanClick을 true로 만들고 해당 유닛을 파티에 추가합니다.
    /// RobotFactory에서 구매 조건 충족 시 호출합니다.
    /// </summary>
    public static void UnlockAndAdd(Unit targetUnit)
    {
        // 1. 모든 FactoryAddUnit 인스턴스에 CanClick = true 브로드캐스트
        OnUnlockAll?.Invoke();

        // 2. 해당 유닛을 가진 FactoryAddUnit을 찾아 AddUnitToParty 호출
        FactoryAddUnit[] all = FindObjectsOfType<FactoryAddUnit>();
        foreach (FactoryAddUnit fau in all)
        {
            if (fau.MyUnit == targetUnit)
            {
                fau.AddUnitToParty();
                return;
            }
        }
    }

    public void AddUnitToParty()
    {
        if (MyUnit != null && CanClick)
        {
            PartyManager.instance.Add(MyUnit);
            MyUnit.gameObject.SetActive(false);
            Debug.Log($"[FactoryAddUnit] {MyUnit.Unitname} 파티에 추가 완료!");
        }
    }
}
