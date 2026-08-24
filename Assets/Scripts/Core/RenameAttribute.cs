using UnityEngine;

public class RenameAttribute : PropertyAttribute
{
    public string NewName;
    public RenameAttribute(string name)
    {
        NewName = name;
    }
}