using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 1.2f;
    public float lifeTime = 0.9f;

    [HideInInspector]
    public bool isPooled = false;

    private float maxLifeTime;
    private TMP_Text textMesh;
    private Color baseColor = Color.white;

    private void Awake()
    {
        EnsureCached();
    }

    private void EnsureCached()
    {
        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TMP_Text>();
            if (textMesh != null)
            {
                baseColor = textMesh.color;
            }
        }
        if (maxLifeTime <= 0)
        {
            maxLifeTime = lifeTime > 0 ? lifeTime : 0.9f;
        }
    }

    /// <summary>
    /// 풀에서 재사용될 때 위치, 텍스트, 색상 및 수명을 초기화합니다.
    /// </summary>
    public void Init(Vector3 position, string text, Color color)
    {
        transform.position = position;
        EnsureCached();

        lifeTime = maxLifeTime;
        baseColor = color;

        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
        }
    }

    public void SetText(string text, Color color)
    {
        EnsureCached();

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
            if (isPooled)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}