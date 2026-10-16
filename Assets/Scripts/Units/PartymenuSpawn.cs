
using System.Collections.Generic;
using UnityEngine;

/// <summary>파티 유닛 프리팹을 지정된 위치에 생성한다.</summary>
public class PartymenuSpawn : MonoBehaviour
{
    private static readonly List<PartymenuSpawn> activeSpawners =
        new List<PartymenuSpawn>();

    [Header("Party Slots")]
    [SerializeField] private GameObject partySettingUI;
    public Unit[] PartySlotUI = new Unit[4];
    public Transform[] PartySlotPosition = new Transform[4];

    private readonly Unit[] spawnedUnits = new Unit[4];

    private void Awake()
    {
        if (!activeSpawners.Contains(this))
            activeSpawners.Add(this);

        ResolvePanelAndSlots();
    }

    private void Start()
    {
        PartySlot();
    }

    private void OnDestroy()
    {
        activeSpawners.Remove(this);
    }
    /// <summary>파티 설정 패널 상태 변경 시 호출한다.</summary>
    public static void NotifyPanelState(GameObject panel)
    {
        if (panel == null || panel.name != "Party Setting UI")
            return;

        PartymenuSpawn spawner = FindSpawnerFor(panel);

        if (spawner == null)
            spawner = panel.AddComponent<PartymenuSpawn>();

        spawner.partySettingUI = panel;
        spawner.ResolvePanelAndSlots();

        if (panel.activeInHierarchy)
            spawner.PartySpawn();
        else
            spawner.ClearSpawnedUnits();
    }

    private static PartymenuSpawn FindSpawnerFor(GameObject panel)
    {
        foreach (PartymenuSpawn spawner in activeSpawners)
        {
            if (spawner != null && spawner.IsForPanel(panel))
                return spawner;
        }

        return panel.GetComponent<PartymenuSpawn>();
    }

    private bool IsForPanel(GameObject panel)
    {
        if (partySettingUI == panel)
            return true;

        foreach (Transform slot in PartySlotPosition)
        {
            if (slot != null && slot.IsChildOf(panel.transform))
                return true;
        }

        return false;
    }

    private void ResolvePanelAndSlots()
    {
        if (partySettingUI == null || PartySlotPosition == null)
            return;

        for (int i = 0; i < PartySlotPosition.Length; i++)
        {
            if (PartySlotPosition[i] == null)
                continue;

            if (PartySlotPosition[i].IsChildOf(partySettingUI.transform))
                continue;

            Transform found = partySettingUI.transform.Find($"Slot {i + 1}");

            if (found != null)
                PartySlotPosition[i] = found;
        }
    }

    public void PartySlot()
    {
        if (PartyManager.instance == null)
        {
            Debug.LogWarning(
                "[PartymenuSpawn] PartyManager 인스턴스를 찾을 수 없습니다."
            );
            return;
        }

        for (int i = 0; i < PartySlotUI.Length; i++)
        {
            PartySlotUI[i] =
                i < PartyManager.instance.partySlots.Length
                ? PartyManager.instance.partySlots[i]
                : null;
        }
        Debug.Log("[PartymenuSpawn] PartySlotUI 배열이 업데이트되었습니다.");
    }

    /// <summary>일반 월드 오브젝트로 파티 유닛을 생성한다.</summary>
    public void PartySpawn()
    {
        ResolvePanelAndSlots();
        PartySlot();

        for (int i = 0; i < PartySlotUI.Length; i++)
        {
            if (spawnedUnits[i] != null)
            {
                Destroy(spawnedUnits[i].gameObject);
                spawnedUnits[i] = null;
            }

            if (PartySlotUI[i] == null ||
                i >= PartySlotPosition.Length ||
                PartySlotPosition[i] == null)
            {
                continue;
            }

            Transform slot = PartySlotPosition[i];

            // 슬롯의 위치와 회전을 기준으로 일반 오브젝트 생성
            Unit spawned = Instantiate(
                PartySlotUI[i],
                slot.position,
                slot.rotation
            );

            spawned.name = PartySlotUI[i].name;
            spawned.transform.localScale = new Vector3(
                1f,
                1f,
                1f
            );

            spawnedUnits[i] = spawned;
        }
        Debug.Log("[PartymenuSpawn] PartySpawn 메서드가 완료되었습니다.");
    }

    private void ClearSpawnedUnits()
    {
        for (int i = 0; i < spawnedUnits.Length; i++)
        {
            if (spawnedUnits[i] != null)
                Destroy(spawnedUnits[i].gameObject);

            spawnedUnits[i] = null;
        }
    }
}
