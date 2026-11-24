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

    /// <summary>
    /// UI 오브젝트를 활성화하고 팝업 애니메이션을 실행합니다.
    /// 설정창(Canvas 또는 Setting)의 경우 피벗(0,0)에 의해 왼쪽에서 튕겨오는 왜곡을 방지하기 위해
    /// 캔버스 자체는 (1,1,1)로 활성화하고 내부 패널(SettingUI, 중앙 피벗)을 부드럽게 팝업합니다.
    /// 일반 UI는 기존 Ease.OutBack 연출을 그대로 유지합니다.
    /// </summary>
    private void AnimateOpen(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(true);
        PartymenuSpawn.NotifyPanelState(obj);

        Canvas canvas = obj.GetComponent<Canvas>();
        bool isSettingUI = canvas != null || obj.name.Contains("Setting");

        if (isSettingUI)
        {
            // 캔버스 자체의 스케일은 항상 1로 고정하여 화면 왜곡 방지
            obj.transform.localScale = Vector3.one;

            // 실제 애니메이션을 적용할 내부 패널 탐색 (SettingUI 등 중앙 피벗 객체)
            Transform panelTarget = null;
            if (canvas != null)
            {
                Transform settingChild = obj.transform.Find("SettingUI");
                panelTarget = settingChild != null ? settingChild : (obj.transform.childCount > 0 ? obj.transform.GetChild(0) : obj.transform);
            }
            else
            {
                panelTarget = obj.transform;
            }

            panelTarget.DOKill();
            Vector3 origScale = GetOriginalScale(panelTarget);
            panelTarget.localScale = origScale * startScale;

            // 설정창만 왼쪽 쏠림과 과한 튕김 없이 부드러운 OutCubic으로 자연스럽게 중앙 팝업
            panelTarget.DOScale(origScale, popupDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
            return;
        }

        // 일반 UI: 기존 로직 및 Ease.OutBack 그대로 유지
        Transform target = obj.transform;
        target.DOKill();

        Vector3 originalScale = GetOriginalScale(target);
        target.localScale = originalScale * startScale;

        target.DOScale(originalScale, popupDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    private void AnimateClose(GameObject obj)
    {
        if (obj == null) return;

        obj.transform.DOKill();

        Transform settingChild = obj.transform.Find("SettingUI");
        if (settingChild != null)
            settingChild.DOKill();

        obj.SetActive(false);
        PartymenuSpawn.NotifyPanelState(obj);
    }

    public void OpenAll()
    {
        if (uiObjects == null) return;

        foreach (GameObject obj in uiObjects)
        {
            if (obj == null) continue;
            AnimateOpen(obj);
        }
    }

    public void OpenThis(GameObject thisObject)
    {
        if (thisObject == null) return;
        AnimateOpen(thisObject);
    }

    public void CloseThis(GameObject thisObject)
    {
        if (thisObject == null) return;
        AnimateClose(thisObject);
    }

    public void CloseAll()
    {
        if (uiObjects == null) return;

        foreach (GameObject obj in uiObjects)
        {
            if (obj == null) continue;
            AnimateClose(obj);
        }
    }

    public void ToggleAll()
    {
        if (uiObjects == null) return;

        foreach (GameObject obj in uiObjects)
        {
            if (obj == null) continue;

            if (obj.activeSelf)
                AnimateClose(obj);
            else
                AnimateOpen(obj);
        }
    }

    public void ToggleThis(GameObject thisObject)
    {
        if (thisObject == null) return;

        if (thisObject.activeSelf)
            AnimateClose(thisObject);
        else
            AnimateOpen(thisObject);
    }

    private Vector3 GetOriginalScale(Transform target)
    {
        if (!originalScales.TryGetValue(target, out Vector3 scale) || scale == Vector3.zero)
        {
            scale = target.localScale == Vector3.zero ? Vector3.one : target.localScale;
            originalScales[target] = scale;
        }

        return scale;
    }

    // ==========================================
    // 설정(Setting) 전용 메서드
    // ==========================================

    public void OnSetting()
    {
        if (uiObjects != null && uiObjects.Length > 0)
        {
            OpenAll();
            return;
        }

        if (AudioManager.instance != null)
        {
            Canvas canvas = AudioManager.instance.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                AnimateOpen(canvas.gameObject);
                return;
            }
            AudioManager.instance.OnChild();
        }
    }

    public void OffSetting()
    {
        if (uiObjects != null && uiObjects.Length > 0)
        {
            CloseAll();
            return;
        }

        if (AudioManager.instance != null)
        {
            Canvas canvas = AudioManager.instance.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                AnimateClose(canvas.gameObject);
                return;
            }
            AudioManager.instance.OffChild();
        }
    }

    public void ToggleSetting()
    {
        if (uiObjects != null && uiObjects.Length > 0)
        {
            ToggleAll();
            return;
        }

        if (AudioManager.instance != null)
        {
            Canvas canvas = AudioManager.instance.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                if (canvas.gameObject.activeSelf)
                    OffSetting();
                else
                    OnSetting();
            }
        }
    }
}
