using UnityEngine;

public class FocusableObject : MonoBehaviour
{
    // 각 오브젝트마다 독립적으로 저장되는 원래 위치 정보
    public Vector3 OriginalPosition { get; private set; }
    public Vector3 OriginalScale { get; private set; }
    public Quaternion OriginalRotation { get; private set; }
    public int OriginalSiblingIndex { get; private set; }

    public bool IsFocused { get; set; } = false;

    private void Awake()
    {
        SaveOriginalState();
    }

    public void SaveOriginalState()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            OriginalPosition = rect.anchoredPosition;
            OriginalSiblingIndex = rect.GetSiblingIndex();
        }
        else
        {
            OriginalPosition = transform.position;
        }

        OriginalScale = transform.localScale;
        OriginalRotation = transform.rotation;
    }
}