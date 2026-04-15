using UnityEngine;

public class Partybase : MonoBehaviour
{
    public GameObject[] party;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void addParty(GameObject newPartyMember)
    {
        if(party.Length < 4)
        {
            party[party.Length] = newPartyMember;
        }
        else
        {
            Debug.Log("파티가 가득 찼습니다!");
        }
    }
    public void removeParty(GameObject partyMember)
    {
        for(int i = 0; i < party.Length; i++)
        {
            if(party[i] == partyMember)
            {
                party[i] = null;
                break;
            }
        }
    }
}