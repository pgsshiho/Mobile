using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(RenameAttribute))]
public class RenameDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        RenameAttribute rename = (RenameAttribute)attribute;
        label.text = rename.NewName;
        EditorGUI.PropertyField(position, property, label, true);
    }
}