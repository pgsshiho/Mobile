using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUIHandler
{
    private readonly Unit owner;
    private readonly GameObject damageTextPrefab;
    private readonly Transform damageTextSpawnPoint;
    private readonly GameObject myturnUI;
    private readonly Transform statusIconParent;
    private readonly StatusIconSet statusIconSet;
    private readonly GameObject statusIconPrefab;
    private readonly StatusIconData[] statusIconDatas;

    private readonly Dictionary<StatusType, GameObject> statusIcons = new Dictionary<StatusType, GameObject>();
    private Transform dynamicIconContainer;

    // 생성자 1: StatusIconSet 사용
    public UnitUIHandler(
        Unit owner,
        GameObject damageTextPrefab,
        Transform damageTextSpawnPoint,
        GameObject myturnUI,
        Transform statusIconParent,
        StatusIconSet statusIconSet)
    {
        this.owner = owner;
        this.damageTextPrefab = damageTextPrefab;
        this.damageTextSpawnPoint = damageTextSpawnPoint;
        this.myturnUI = myturnUI;
        this.statusIconParent = statusIconParent;
        this.statusIconSet = statusIconSet;
    }

    // 생성자 2: 레거시 개별 프리팹 및 배열 사용
    public UnitUIHandler(
        Unit owner,
        GameObject damageTextPrefab,
        Transform damageTextSpawnPoint,
        GameObject myturnUI,
        Transform statusIconParent,
        GameObject statusIconPrefab,
        StatusIconData[] statusIconDatas)
    {
        this.owner = owner;
        this.damageTextPrefab = damageTextPrefab;
        this.damageTextSpawnPoint = damageTextSpawnPoint;
        this.myturnUI = myturnUI;
        this.statusIconParent = statusIconParent;
        this.statusIconPrefab = statusIconPrefab;
        this.statusIconDatas = statusIconDatas;
    }

    public void SetTurnUI(bool active)
    {
        if (myturnUI != null)
        {
            myturnUI.SetActive(active);
        }
    }

    /// <summary>
    /// 피격, 회복, 지속 데미지 수치를 플로팅 텍스트로 화면에 표시합니다.
    /// </summary>
    public void ShowDamageText(int damage, Unit.DamageType type)
    {
        if (owner == null) return;

        Transform spawnPoint = damageTextSpawnPoint != null ? damageTextSpawnPoint : owner.transform;
        Vector3 spawnPos = spawnPoint.position + Vector3.up * 1.0f + new Vector3(Random.Range(-0.2f, 0.2f), 0f, 0f);

        Color textColor;
        string displayText = damage.ToString();

        switch (type)
        {
            case Unit.DamageType.Bleed:
                textColor = new Color(0.95f, 0.15f, 0.15f);
                break;
            case Unit.DamageType.Fire:
                textColor = new Color(1f, 0.45f, 0.05f);
                break;
            case Unit.DamageType.Heal:
                textColor = new Color(0.2f, 1f, 0.3f);
                displayText = "+" + damage;
                break;
            case Unit.DamageType.Corrosion:
                textColor = new Color(0.75f, 0.45f, 0.2f);
                break;
            case Unit.DamageType.Electric:
                textColor = new Color(1f, 0.95f, 0.15f);
                break;
            default:
                textColor = Color.white;
                break;
        }

        GameObject prefab = damageTextPrefab;
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Prefabs/UI/Text (TMP)");
        }

        GameObject obj = null;
        if (prefab != null)
        {
            obj = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // 프리팹이 없을 경우 월드 텍스트 오브젝트 동적 생성
            obj = new GameObject("DamageText_Dynamic");
            obj.transform.position = spawnPos;
            var textComp = obj.AddComponent<TextMeshPro>();
            textComp.fontSize = 6;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.sortingOrder = 50;
        }

        if (obj != null)
        {
            // TextMeshProUGUI가 월드에 독립 생성된 경우 렌더링을 위해 World Canvas 자동 보정
            var tmpUGUI = obj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpUGUI != null && obj.GetComponentInParent<Canvas>() == null)
            {
                Canvas worldCanvas = obj.AddComponent<Canvas>();
                worldCanvas.renderMode = RenderMode.WorldSpace;
                worldCanvas.sortingOrder = 50;

                var scaler = obj.AddComponent<CanvasScaler>();
                scaler.dynamicPixelsPerUnit = 10;

                RectTransform rt = obj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.sizeDelta = new Vector2(200, 50);
                    rt.localScale = Vector3.one * 0.015f;
                }
            }

            DamageText dmgText = obj.GetComponent<DamageText>() ?? obj.AddComponent<DamageText>();
            dmgText.SetText(displayText, textColor);
        }
    }

    /// <summary>
    /// 상태이상 아이콘을 유닛 머리 위에 생성하고 정렬합니다.
    /// </summary>
    public void AddStatusIcon(StatusType type)
    {
        if (statusIcons.ContainsKey(type))
            return;

        if (owner == null)
            return;

        Sprite icon = GetStatusIcon(type);
        if (icon == null)
        {
            Debug.LogWarning($"[UnitUIHandler] {owner.Unitname}의 상태이상 [{type}] 아이콘 스프라이트를 찾을 수 없습니다.");
            return;
        }

        Transform parent = GetOrCreateIconContainer();
        if (parent == null)
            return;

        GameObject prefabToUse = (statusIconSet != null) ? statusIconSet.statusIconPrefab : statusIconPrefab;
        GameObject obj;

        if (prefabToUse != null)
        {
            obj = Object.Instantiate(prefabToUse, parent);
        }
        else
        {
            // 기본 스프라이트 렌더러 아이콘 동적 생성
            obj = new GameObject($"StatusIcon_{type}");
            obj.transform.SetParent(parent, false);

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = icon;

            // 유닛 스프라이트 정렬 레이어 상속
            SpriteRenderer unitSr = owner.GetComponent<SpriteRenderer>();
            if (unitSr != null)
            {
                sr.sortingLayerID = unitSr.sortingLayerID;
                sr.sortingOrder = unitSr.sortingOrder + 15;
            }
            else
            {
                sr.sortingOrder = 25;
            }

            obj.transform.localScale = Vector3.one * 0.15f;
        }

        // Image 컴포넌트가 있으면 스프라이트 지정
        Image image = obj.GetComponentInChildren<Image>();
        if (image != null)
        {
            image.sprite = icon;
        }

        // SpriteRenderer 컴포넌트가 있으면 스프라이트 지정
        SpriteRenderer spriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = icon;
        }

        statusIcons.Add(type, obj);
        RepositionStatusIcons();

        Debug.Log($"<color=cyan>[UnitUIHandler]</color> {owner.Unitname} [{type}] 상태이상 아이콘 표시 완료!");
    }

    /// <summary>
    /// 상태이상 해제 시 해당 아이콘을 제거하고 남은 아이콘들을 재정렬합니다.
    /// </summary>
    public void RemoveStatusIcon(StatusType type)
    {
        if (!statusIcons.TryGetValue(type, out GameObject iconObj))
            return;

        if (iconObj != null)
        {
            Object.Destroy(iconObj);
        }

        statusIcons.Remove(type);
        RepositionStatusIcons();
    }

    public void ClearStatusIcons()
    {
        foreach (GameObject icon in statusIcons.Values)
        {
            if (icon != null)
            {
                Object.Destroy(icon);
            }
        }

        statusIcons.Clear();
    }

    private Transform GetOrCreateIconContainer()
    {
        if (statusIconParent != null)
            return statusIconParent;

        if (dynamicIconContainer != null)
            return dynamicIconContainer;

        if (owner == null)
            return null;

        Transform existing = owner.transform.Find("StatusIcon_Container");
        if (existing != null)
        {
            dynamicIconContainer = existing;
            return dynamicIconContainer;
        }

        GameObject containerObj = new GameObject("StatusIcon_Container");
        containerObj.transform.SetParent(owner.transform, false);
        containerObj.transform.localPosition = new Vector3(0f, 1.9f, 0f);
        dynamicIconContainer = containerObj.transform;

        return dynamicIconContainer;
    }

    private void RepositionStatusIcons()
    {
        float spacing = 0.17f;
        int total = statusIcons.Count;
        if (total == 0) return;

        float startX = -(total - 1) * spacing * 0.5f;
        int index = 0;

        foreach (var kvp in statusIcons)
        {
            if (kvp.Value != null)
            {
                kvp.Value.transform.localPosition = new Vector3(startX + index * spacing, 0f, 0f);
                index++;
            }
        }
    }

    private Sprite GetStatusIcon(StatusType type)
    {
        if (statusIconSet != null)
        {
            Sprite s = statusIconSet.GetIcon(type);
            if (s != null) return s;
        }

        if (statusIconDatas != null)
        {
            foreach (StatusIconData data in statusIconDatas)
            {
                if (data != null && data.statusType == type && data.icon != null)
                {
                    return data.icon;
                }
            }
        }

        // Resources 폴백
        StatusIconSet defaultSet = Resources.Load<StatusIconSet>("ScriptableObjects/StatusEffects/StatusIconSet");
        if (defaultSet != null)
        {
            Sprite s = defaultSet.GetIcon(type);
            if (s != null) return s;
        }

        return null;
    }
}
