using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FactoryAddUnit : MonoBehaviour, IPointerClickHandler
{
    [Header("Unit Reference")]
    public Unit MyUnit;

    [Header("State")]
    [Tooltip("구매 완료 후 로봇 선택 가능 여부")]
    public bool CanClick = false;

    [Header("Optional UI Button")]
    [Tooltip("버튼 컴포넌트가 있을 경우 자동 또는 수동 연결")]
    public Button selectButton;

    // ── 정적 Action: 씬의 모든 FactoryAddUnit 인스턴스에게 브로드캐스트 ──
    public static event Action<bool> OnSetCanClickAll;

    private void Awake()
    {
        if (selectButton == null)
        {
            selectButton = GetComponent<Button>();
        }

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnClickAddUnit);
        }
    }

    private void OnEnable()
    {
        OnSetCanClickAll += SetCanClickState;
    }

    private void OnDisable()
    {
        OnSetCanClickAll -= SetCanClickState;
    }

    private void SetCanClickState(bool canClick)
    {
        CanClick = canClick;
    }

    /// <summary>
    /// 씬 내 모든 FactoryAddUnit의 CanClick을 true로 활성화합니다. (구매 완료 시 호출)
    /// </summary>
    public static void UnlockAll()
    {
        OnSetCanClickAll?.Invoke(true);
        Debug.Log("[FactoryAddUnit] 모든 로봇 선택 가능 상태로 활성화 (CanClick = true)");
    }

    /// <summary>
    /// 씬 내 모든 FactoryAddUnit의 CanClick을 false로 비활성화합니다. (유닛 추가 완료 시 호출)
    /// </summary>
    public static void LockAll()
    {
        OnSetCanClickAll?.Invoke(false);
        Debug.Log("[FactoryAddUnit] 모든 로봇 선택 상태 잠금 (CanClick = false)");
    }

    /// <summary>
    /// 마우스/터치 클릭 이벤트 (3D 콜라이더 또는 UI)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickAddUnit();
    }

    /// <summary>
    /// 버튼 OnClick 이벤트 또는 코드에서 호출할 수 있는 유닛 추가 메서드입니다.
    /// </summary>
    public void OnClickAddUnit()
    {
        AddUnitToParty();
    }

    public void AddUnitToParty()
    {
        if (MyUnit == null)
        {
            Debug.LogWarning("[FactoryAddUnit] 연결된 MyUnit이 없습니다.");
            return;
        }

        if (!CanClick)
        {
            Debug.Log("[FactoryAddUnit] 먼저 로봇을 구매해야 유닛을 추가할 수 있습니다. (CanClick is false)");
            return;
        }

        if (PartyManager.instance == null) return;

        if (PartyManager.instance.IsPartyFull())
        {
            Debug.LogWarning("[FactoryAddUnit] 파티 슬롯(4명)이 이미 가득 찼습니다!");
            return;
        }

        bool success = PartyManager.instance.Add(MyUnit);
        if (success)
        {
            PartyManager.instance.SaveParty();
            MyUnit.gameObject.SetActive(false);
            gameObject.SetActive(false);

            // 유닛 1명을 추가했으므로 다시 모든 선택 상태를 잠급니다.
            LockAll();

            Debug.Log($"<color=green>[FactoryAddUnit]</color> {MyUnit.Unitname}이(가) 파티에 성공적으로 추가되었습니다!");
        }
        else
        {
            Debug.LogWarning($"[FactoryAddUnit] {MyUnit.Unitname} 파티 추가 실패 (이미 존재하거나 슬롯 없음)");
        }
    }
}
