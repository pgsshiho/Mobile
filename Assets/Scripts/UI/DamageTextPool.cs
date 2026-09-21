using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중 빈번하게 생성/파괴되는 DamageText 오브젝트를 재사용하는 모바일 최적화 오브젝트 풀입니다.
/// </summary>
public class DamageTextPool : MonoBehaviour
{
    private static DamageTextPool instance;
    public static DamageTextPool Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject poolObj = new GameObject("[DamageTextPool]");
                instance = poolObj.AddComponent<DamageTextPool>();
                DontDestroyOnLoad(poolObj);
            }
            return instance;
        }
    }

    private readonly List<DamageText> pool = new List<DamageText>();
    private GameObject cachedPrefab;

    public void SetPrefab(GameObject prefab)
    {
        if (prefab != null && cachedPrefab == null)
        {
            cachedPrefab = prefab;
        }
    }

    /// <summary>
    /// 풀에서 비활성 텍스트를 재사용하거나 필요 시 생성하여 반환합니다.
    /// </summary>
    public DamageText Spawn(GameObject fallbackPrefab, Vector3 position, string text, Color color)
    {
        if (cachedPrefab == null && fallbackPrefab != null)
        {
            cachedPrefab = fallbackPrefab;
        }

        // 1. 비활성화된 기존 오브젝트 탐색
        for (int i = 0; i < pool.Count; i++)
        {
            DamageText item = pool[i];
            if (item != null && !item.gameObject.activeSelf)
            {
                item.transform.position = position;
                item.gameObject.SetActive(true);
                item.Init(position, text, color);
                return item;
            }
        }

        // 2. 풀에 없으면 새로 생성하여 풀에 등록
        GameObject obj = null;
        if (cachedPrefab != null)
        {
            obj = Instantiate(cachedPrefab, position, Quaternion.identity, transform);
        }
        else
        {
            obj = new GameObject("DamageText_Dynamic");
            obj.transform.position = position;
            obj.transform.SetParent(transform);
            var textComp = obj.AddComponent<TMPro.TextMeshPro>();
            textComp.fontSize = 6;
            textComp.alignment = TMPro.TextAlignmentOptions.Center;
            textComp.sortingOrder = 50;
        }

        // TextMeshProUGUI 대응
        var tmpUGUI = obj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmpUGUI != null && obj.GetComponentInParent<Canvas>() == null)
        {
            Canvas worldCanvas = obj.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.sortingOrder = 50;

            var scaler = obj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = new Vector2(200, 50);
                rt.localScale = Vector3.one * 0.015f;
            }
        }

        DamageText dmgText = obj.GetComponent<DamageText>() ?? obj.AddComponent<DamageText>();
        dmgText.isPooled = true;
        dmgText.Init(position, text, color);
        pool.Add(dmgText);

        return dmgText;
    }
}
