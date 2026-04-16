using UnityEngine;

public class OpenFactoryFacillity : MonoBehaviour
{
    public GameObject reapir;
    public GameObject iron;
    public GameObject hard;
    public GameObject charging;
    public GameObject robot;
    public void repairShop()
    {
        reapir.SetActive(true);
    }
    public void ironworks()
    {
        iron.SetActive(true);
    }
    public void hardwarestore()
    {
        hard.SetActive(true);
    }
    public void chargingstation()
    {
        charging.SetActive(true);
    }
    public void robotFactory()
    {
        robot.SetActive(true);
    }
    public void turnoff()
    {
        reapir.SetActive(false);
        iron.SetActive(false);
        hard.SetActive(false);
        charging.SetActive(false);
        robot.SetActive(false);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
