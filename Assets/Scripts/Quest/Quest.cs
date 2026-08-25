using Unity.VisualScripting;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public static Quest instance;
    public void Awake()
    {
        instance = this;
    }
}
