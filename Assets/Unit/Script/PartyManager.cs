using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public List<UnitBase> partyMembers = new List<UnitBase>();
    public GameObject[] unitPrefabs; // 인스펙터에서 할당

    public void AddMember(int index)
    {
        if (partyMembers.Count < 4)
        {
            GameObject obj = Instantiate(unitPrefabs[index]);
            partyMembers.Add(obj.GetComponent<UnitBase>());
        }
    }
    public void RemoveMember(UnitBase member)
    {
        if (partyMembers.Contains(member))
        {
            partyMembers.Remove(member);
            Destroy(member.gameObject);
        }
    }
}