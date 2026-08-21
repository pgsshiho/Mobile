using UnityEngine;

[CreateAssetMenu(menuName = "Status/Status Icon")]
public class StatusIconData : ScriptableObject
{
    public StatusType statusType;
    public Sprite icon;
}