using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    List<GameObject> partyMembers = new List<GameObject>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void addParty()
    {
        if(partyMembers.Count < 4)
        {
            GameObject newMember = new GameObject("PartyMember" + (partyMembers.Count + 1));
            partyMembers.Add(newMember);
            Debug.Log("Added: " + newMember.name);
        }
        else
        {
            Debug.Log("Party is full!");
        }
    }
    public void removeParty()
    {
        if(partyMembers.Count > 0)
        {
            GameObject removedMember = partyMembers[partyMembers.Count - 1];
            partyMembers.RemoveAt(partyMembers.Count - 1);
            Debug.Log("Removed: " + removedMember.name);
        }
        else
        {
            Debug.Log("No members to remove!");
        }
    }
}
