using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    int money = 0; // 상점용
    int material = 0; // 대장간용
    
    public static ResourceManager instance;
    public void Awake()
    {
        instance = this;
    }

}
