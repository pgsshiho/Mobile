using UnityEngine;

public class TurnOffUIForPrefabs: MonoBehaviour
{
    public GameObject[] TurnOffs;

    public TurnOffUIForPrefabs(GameObject[] objects)
    {
        TurnOffs = objects;
    }
}