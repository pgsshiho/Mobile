using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager instance;

    public Unit[] partySlots = new Unit[4];

    private void Awake()
    {
        instance = this;
    }
    
}