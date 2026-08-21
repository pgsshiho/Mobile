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
        for (int i = 0; i < partySlots.Length; i++)
        {
            if (partySlots[i] != null)
            {
                // Unit 프리팹/ScriptableObject의 name을 저장
                PlayerPrefs.SetString($"PartySlot{i}", partySlots[i].name);
            }
            else
            {
                // 빈 슬롯이면 기존 저장 데이터 삭제
                PlayerPrefs.DeleteKey($"PartySlot{i}");
            }
        }
        PlayerPrefs.Save();
    }
    public void LoadParty()
    {
        bool hasAnyData = false; // 저장된 데이터가 하나라도 있는지 체크

        for (int i = 0; i < partySlots.Length; i++)
        {
            // 기본값을 ""(빈 문자열)로 설정하는 것이 안전합니다.
            string unitName = PlayerPrefs.GetString($"PartySlot{i}", "");

            if (!string.IsNullOrEmpty(unitName))
            {
                Unit unit = Resources.Load<Unit>($"Unit/{unitName}");
                if (unit != null)
                {
                    partySlots[i] = unit;
                    hasAnyData = true; // 유효한 데이터가 존재함
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
}