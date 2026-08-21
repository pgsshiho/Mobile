using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public float speed = 1f;
    public float lifeTime = 1f;

    void Update()
    {
        transform.position +=
            Vector3.up * speed * Time.deltaTime;

        lifeTime -= Time.deltaTime;

        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}