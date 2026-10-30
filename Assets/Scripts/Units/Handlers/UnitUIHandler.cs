
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

    private readonly Dictionary<StatusType, GameObject> statusIcons =
        new Dictionary<StatusType, GameObject>();

    // DamageText 프리팹의 World Space Canvas 스케일(0.015) 기준 약 100 UI 픽셀 간격입니다.
    private const float FloatingTextHorizontalSpacing = 1.2f;
    // 상태이상 이름은 숫자 피해 텍스트보다 한글 두 글자 정도(약 30 UI 픽셀) 더 오른쪽에 둡니다.
    private const float StatusTextAdditionalHorizontalOffset = 0.45f;
    private const float FloatingTextLifetime = 0.9f;
    private int floatingTextSequence;
    private float lastFloatingTextTime = -999f;

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

    // ============================================================
    // 데미지 및 전투 텍스트
    // ============================================================

    /// <summary>
    /// 피격, 회복, 지속 데미지 수치를 표시합니다.
    /// </summary>
    public void ShowDamageText(int damage, Unit.DamageType type)
    {
        if (owner == null)
            return;

        string displayText = damage.ToString();
        Color textColor;

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

            case Unit.DamageType.Status:
                textColor = new Color(0.85f, 0.35f, 1f);
                break;

            default:
                textColor = Color.white;
                break;
        }

        SpawnFloatingText(displayText, textColor);
    }

    /// <summary>
    /// 빗나감, 상태이상, 기절, 시스템 오류 등의 전투 메시지를 표시합니다.
    /// </summary>
    public void ShowCombatText(string message, Color textColor)
    {
        if (owner == null || string.IsNullOrEmpty(message))
            return;

        SpawnFloatingText(message, textColor);
    }

    /// <summary>
    /// 상태이상 이름은 숫자 피해 텍스트와 겹치지 않도록 추가 X 오프셋을 적용해 표시합니다.
    /// </summary>
    public void ShowStatusText(string message, Color textColor)
    {
        if (owner == null || string.IsNullOrEmpty(message))
            return;

        SpawnFloatingText(message, textColor, StatusTextAdditionalHorizontalOffset);
    }

    /// <summary>
    /// 플로팅 텍스트 생성 처리를 공통으로 수행합니다.
    /// </summary>
    private void SpawnFloatingText(string message, Color textColor, float additionalHorizontalOffset = 0f)
    {
        if (owner == null)
            return;

        Transform spawnPoint = damageTextSpawnPoint != null
            ? damageTextSpawnPoint
            : owner.transform;

        float horizontalOffset = GetNextFloatingTextHorizontalOffset() + additionalHorizontalOffset;
        Vector3 spawnPos =
            spawnPoint.position
            + Vector3.up * 1.0f
            + Vector3.right * horizontalOffset;

        GameObject prefab = damageTextPrefab;

        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>(
                "Prefabs/UI/Text (TMP)"
            );
        }

        if (prefab == null)
        {
            Debug.LogWarning(
                "[UnitUIHandler] 데미지 텍스트 프리팹을 찾을 수 없습니다."
            );
            return;
        }

        if (DamageTextPool.Instance == null)
        {
            Debug.LogWarning(
                "[UnitUIHandler] DamageTextPool 인스턴스가 없습니다."
            );
            return;
        }

        DamageTextPool.Instance.Spawn(
            prefab,
            spawnPos,
            message,
            textColor
        );
    }

    /// <summary>
    /// 동시에 나온 텍스트를 중앙, 오른쪽, 왼쪽 순서로 배치합니다.
    /// 인접 텍스트 사이의 X 간격은 최소 100 UI 픽셀(1.5 월드 단위)입니다.
    /// </summary>
    private float GetNextFloatingTextHorizontalOffset()
    {
        if (Time.time - lastFloatingTextTime > FloatingTextLifetime)
            floatingTextSequence = 0;

        lastFloatingTextTime = Time.time;
        int index = floatingTextSequence++;
        int horizontalStep = index == 0
            ? 0
            : (index % 2 == 1 ? (index + 1) / 2 : -(index / 2));

        return horizontalStep * FloatingTextHorizontalSpacing;
    }

    // ============================================================
    // 상태이상 아이콘
    // ============================================================

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
            Debug.LogWarning(
                $"[UnitUIHandler] {owner.Unitname}의 상태이상 " +
                $"[{type}] 아이콘 스프라이트를 찾을 수 없습니다."
            );
            return;
        }

        Transform parent = GetOrCreateIconContainer();

        if (parent == null)
            return;

        GameObject prefabToUse =
            statusIconSet != null
                ? statusIconSet.statusIconPrefab
                : statusIconPrefab;

        GameObject obj;

        if (prefabToUse != null)
        {
            obj = Object.Instantiate(prefabToUse, parent);
        }
        else
        {
            obj = new GameObject($"StatusIcon_{type}");
            obj.transform.SetParent(parent, false);

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = icon;

            SpriteRenderer unitSr =
                owner.GetComponent<SpriteRenderer>();

            if (unitSr != null)
            {
                sr.sortingLayerID = unitSr.sortingLayerID;
                sr.sortingOrder = unitSr.sortingOrder + 15;
            }
            else
            {
                sr.sortingOrder = 25;
            }

            // 상태 아이콘이 전투 배경과 겹쳐 묻히지 않도록 충분한 크기와 정렬 순서를 준다.
            obj.transform.localScale = Vector3.one * 0.28f;
            obj.transform.localPosition = Vector3.zero;
        }

        Image image = obj.GetComponentInChildren<Image>();

        if (image != null)
        {
            image.sprite = icon;
        }

        SpriteRenderer spriteRenderer =
            obj.GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = icon;
        }

        statusIcons.Add(type, obj);
        RepositionStatusIcons();

        Debug.Log(
            $"<color=cyan>[UnitUIHandler]</color> " +
            $"{owner.Unitname} [{type}] 상태이상 아이콘 표시 완료!"
        );
    }

    /// <summary>
    /// 상태이상 해제 시 아이콘을 제거하고 재정렬합니다.
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
        if (statusIconParent != null &&
            statusIconParent != owner.transform)
        {
            return statusIconParent;
        }

        if (dynamicIconContainer != null)
        {
            dynamicIconContainer.localPosition =
                new Vector3(-0.3f, 1.7f, 0f);

            return dynamicIconContainer;
        }

        if (owner == null)
            return null;

        Transform existing =
            owner.transform.Find("StatusIcon_Container");

        if (existing != null)
        {
            dynamicIconContainer = existing;
            dynamicIconContainer.localPosition =
                new Vector3(-0.3f, 1.7f, 0f);

            return dynamicIconContainer;
        }

        GameObject containerObj =
            new GameObject("StatusIcon_Container");

        containerObj.transform.SetParent(owner.transform, false);
        containerObj.transform.localPosition =
            new Vector3(-0.3f, 1.7f, 0f);

        dynamicIconContainer = containerObj.transform;

        return dynamicIconContainer;
    }

    private void RepositionStatusIcons()
    {
        // 여러 상태이상 아이콘은 유닛 위에서 서로 겹치지 않게 가로로 배치한다.
        float spacing = 0.65f;
        int total = statusIcons.Count;

        if (total == 0)
            return;

        float startX = -(total - 1) * spacing * 0.5f;
        int index = 0;

        foreach (var kvp in statusIcons)
        {
            if (kvp.Value != null)
            {
                kvp.Value.transform.localPosition =
                    new Vector3(
                        startX + index * spacing,
                        0f,
                        0f
                    );

                index++;
            }
        }
    }

    private Sprite GetStatusIcon(StatusType type)
    {
        if (statusIconSet != null)
        {
            Sprite s = statusIconSet.GetIcon(type);

            if (s != null)
                return s;
        }

        if (statusIconDatas != null)
        {
            foreach (StatusIconData data in statusIconDatas)
            {
                if (data != null &&
                    data.statusType == type &&
                    data.icon != null)
                {
                    return data.icon;
                }
            }
        }

        StatusIconSet defaultSet =
            Resources.Load<StatusIconSet>(
                "ScriptableObjects/StatusEffects/StatusIconSet"
            );

        if (defaultSet != null)
        {
            Sprite s = defaultSet.GetIcon(type);

            if (s != null)
                return s;
        }

        return null;
    }
}
