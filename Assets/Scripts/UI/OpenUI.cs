
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class OpenUI : MonoBehaviour
{
    [SerializeField] private GameObject[] uiObjects;

    [Header("팝업 애니메이션")]
    [SerializeField] private float popupDuration = 0.25f;

    [SerializeField, Range(0.01f, 1f)]
    private float startScale = 0.7f;

    // 각 오브젝트의 원래 크기 저장
    private readonly Dictionary<Transform, Vector3> originalScales = new();

    private void Awake()
    {
        if (uiObjects == null) return;

        foreach (GameObject obj in uiObjects)
        {
            if (obj == null) continue;

            Transform target = obj.transform;

            if (!originalScales.ContainsKey(target))
                originalScales.Add(target, target.localScale);
        }
    }

    public void OpenAll()
    {
        if (uiObjects == null) return;

        foreach (GameObject obj in uiObjects)
        {
            if (obj == null) continue;

            Transform target = obj.transform;

            target.DOKill();
            obj.SetActive(true);

            PartymenuSpawn.NotifyPanelState(obj);

            Vector3 originalScale = GetOriginalScale(target);

            target.localScale = originalScale * startScale;

            target.DOScale(originalScale, popupDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }

    public void CloseAll()
    {
        if (uiObjects == null) return;

        foreach (GameObject obj in uiObjects)
        {
            if (obj == null) continue;

            obj.transform.DOKill();
            obj.SetActive(false);

            PartymenuSpawn.NotifyPanelState(obj);
        }
    }

    public void ToggleAll()
    {
        if (uiObjects == null) return;

        foreach (GameObject obj in uiObjects)
        {
            if (obj == null) continue;

            if (obj.activeSelf)
            {
                obj.transform.DOKill();
                obj.SetActive(false);
                PartymenuSpawn.NotifyPanelState(obj);
            }
            else
            {
                Transform target = obj.transform;

                target.DOKill();
                obj.SetActive(true);

                PartymenuSpawn.NotifyPanelState(obj);

                Vector3 originalScale = GetOriginalScale(target);

                target.localScale = originalScale * startScale;

                target.DOScale(originalScale, popupDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            }
        }
    }

    private Vector3 GetOriginalScale(Transform target)
    {
        if (!originalScales.TryGetValue(target, out Vector3 scale))
        {
            scale = target.localScale;
            originalScales[target] = scale;
        }

        return scale;
    }

    public void OnSetting()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.OnChild();
    }

    public void OffSetting()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.OffChild();
    }
}
