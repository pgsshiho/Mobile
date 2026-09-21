using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 1.2f;
    public float lifeTime = 0.9f;

    private float maxLifeTime;
    private TMP_Text textMesh;
    private Color baseColor = Color.white;

    private void Awake()
    {
        textMesh = GetComponentInChildren<TMP_Text>();
        if (textMesh != null)
        {
            baseColor = textMesh.color;
        }
        maxLifeTime = lifeTime;
    }

    public void SetText(string text, Color color)
    {
        if (textMesh == null)
            textMesh = GetComponentInChildren<TMP_Text>();

        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            baseColor = color;
        }
    }

    private void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
        lifeTime -= Time.deltaTime;

        if (textMesh != null && maxLifeTime > 0)
        {
            // 후반 40% 시간 동안 페이드 아웃
            float fadeDuration = maxLifeTime * 0.4f;
            if (lifeTime <= fadeDuration)
            {
                float alpha = Mathf.Clamp01(lifeTime / fadeDuration);
                Color c = baseColor;
                c.a = alpha;
                textMesh.color = c;
            }
        }

        if (lifeTime <= 0)
        {
            Destroy(gameObject);
        }
    }
}