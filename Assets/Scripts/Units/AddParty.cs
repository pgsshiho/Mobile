using UnityEngine;
using UnityEngine.EventSystems;

public class AddParty : MonoBehaviour
{
    public Unit NewOne;

    public void Add()
    {
        if (PartyManager.instance.Add(NewOne))
        {
            Debug.Log($"{NewOne.name}을 파티에 추가했습니다.");
        }
    }
}