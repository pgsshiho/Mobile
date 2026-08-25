using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager instance;

    public Unit[] partySlots = new Unit[4];

    private void Awake()
    {
        instance = this;
    }
    public void Start()
    {
        LoadParty();
    }
    public void SaveParty()
    {
        Save.SaveParty(partySlots);
    }
    public void LoadParty()
    {
        bool hasAnyData = Save.HasPartySaveData(partySlots.Length);

        for (int i = 0; i < partySlots.Length; i++)
        {
            string unitName = Save.GetPartySlotUnitName(i);

            if (!string.IsNullOrEmpty(unitName))
            {
                Unit unit = Resources.Load<Unit>($"Unit/{unitName}");
                if (unit != null)
                {
                    partySlots[i] = unit;
                }
                else
                {
                    partySlots[i] = null;
                }
            }
            else
            {
                partySlots[i] = null;
            }
        }

        // 저장된 파티 데이터가 전혀 없을 경우 (최초 실행 등)
        if (!hasAnyData)
        {
            SetDefaultParty();
        }
    }

    // ★ [신규] 기본 파티 설정 메서드
    private void SetDefaultParty()
    {
        Debug.Log("저장된 파티 데이터가 없어 기본 파티를 생성합니다.");

        Unit defaultWarrior = Resources.Load<Unit>("Unit/Cutter");

        if (defaultWarrior != null)
        {
            partySlots[0] = defaultWarrior; 
        }
        SaveParty();
    }
    public bool Add(Unit unit)
    {
        if (unit == null) return false;

        // 에 파티가 가득 찼는지 확인
        bool isFull = true;

        // 이미 파티에 존재하면 추가하지 않음
        for (int i = 0; i < partySlots.Length; i++)
        {
            if (partySlots[i] == unit) return false;
            // 빈 슬롯이 하나라도 있으면 false 반환
            if (partySlots[i] == null) isFull = false;
        }

        // 파티가 가득 찼다면 false 반환
        if (isFull) return false;

        // 첫 빈 슬롯에 추가
        for (int i = 0; i < partySlots.Length; i++)
        {
            if (partySlots[i] == null)
            {
                partySlots[i] = unit;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 파티에서 해당 유닛을 제거합니다. 제거 성공시 true, 없으면 false를 반환합니다.
    /// </summary>
    public bool Remove(Unit unit)
    {
        if (unit == null) return false;

        for (int i = 0; i < partySlots.Length; i++)
        {
            if (partySlots[i] == unit)
            {
                partySlots[i] = null;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 지정된 Transform 위치 배열에 맞추어 파티 유닛들을 씬에 배치(필요시 인스턴스화)합니다.
    /// </summary>
    public void PlacePartyAtPositions(Transform[] positions)
    {
        if (positions == null || positions.Length == 0) return;

        for (int i = 0; i < partySlots.Length; i++)
        {
            if (partySlots[i] == null) continue;

            Transform targetPoint = (i < positions.Length && positions[i] != null) ? positions[i] : null;
            Vector3 targetPos = (targetPoint != null) ? targetPoint.position : Vector3.zero;
            Quaternion targetRot = (targetPoint != null) ? targetPoint.rotation : Quaternion.identity;

            // 씬에 인스턴스화되지 않은 프리팹 에셋인 경우 씬에 Instantiate
            if (!partySlots[i].gameObject.scene.IsValid())
            {
                Unit instance = Instantiate(partySlots[i], targetPos, targetRot, transform);
                instance.name = partySlots[i].name; // 원본 이름 유지
                partySlots[i] = instance;
            }
            else
            {
                // 이미 씬에 존재하는 인스턴스인 경우 위치 및 회전 이동
                if (targetPoint != null)
                {
                    partySlots[i].transform.position = targetPos;
                    partySlots[i].transform.rotation = targetRot;
                }
            }

            partySlots[i].gameObject.SetActive(true);
        }
    }
}