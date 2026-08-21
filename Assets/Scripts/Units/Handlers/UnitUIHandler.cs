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
    private readonly GameObject statusIconPrefab;
    private readonly StatusIconData[] statusIconDatas;

    private readonly Dictionary<StatusType, GameObject> statusIcons = new Dictionary<StatusType, GameObject>();

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

    public void ShowDamageText(int damage, Unit.DamageType type)
    {
        if (damageTextPrefab == null)
            return;

        Transform spawnPoint = damageTextSpawnPoint != null ? damageTextSpawnPoint : owner.transform;

        GameObject obj = Object.Instantiate(
            damageTextPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = damage.ToString();

            switch (type)
            {
                case Unit.DamageType.Bleed:
                    text.color = Color.red;
                    break;

                case Unit.DamageType.Fire:
                    text.color = new Color(1f, 0.45f, 0f);
                    break;

                case Unit.DamageType.Heal:
                    text.color = Color.green;
                    text.text = "+" + damage;
                    break;

                default:
                    text.color = Color.white;
                    break;
            }
        }
    }

    public void AddStatusIcon(StatusType type)
    {
        if (statusIcons.ContainsKey(type))
            return;

        if (statusIconParent == null || statusIconPrefab == null)
            return;

        Sprite icon = GetStatusIcon(type);
        if (icon == null)
            return;

        GameObject obj = Object.Instantiate(statusIconPrefab, statusIconParent);
        Image image = obj.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = icon;
        }

        statusIcons.Add(type, obj);
    }

    public void RemoveStatusIcon(StatusType type)
    {
        if (!statusIcons.TryGetValue(type, out GameObject iconObj))
            return;

        if (iconObj != null)
        {
            Object.Destroy(iconObj);
        }

        statusIcons.Remove(type);
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

    private Sprite GetStatusIcon(StatusType type)
    {
        if (statusIconDatas == null)
            return null;

        foreach (StatusIconData data in statusIconDatas)
        {
            if (data != null && data.statusType == type)
            {
                return data.icon;
            }
        }

        return null;
    }
}
