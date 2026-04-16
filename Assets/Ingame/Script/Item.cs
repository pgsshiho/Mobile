using UnityEngine;
public enum Items
{
    scrap,
    battery,
    antena,
    fuse,
    sapo,
    nok,
    cooler,
    firefighter,
    sol,
    jultape,
    bangtape
}
public class Item : MonoBehaviour
{
    public Items items;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void use()
    {
               switch (items)
        {
            case Items.scrap:
                break;
            case Items.battery:
                break;
            case Items.antena:
                break;
            case Items.fuse:
                break;
            case Items.sapo:
                break;
            case Items.nok:
                break;
            case Items.cooler:
                break;
            case Items.firefighter:
                break;
            case Items.sol:
                break;
            case Items.jultape:
                break;
            case Items.bangtape:
                break;
        }
    }
}
