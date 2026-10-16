using UnityEngine;

[CreateAssetMenu(fileName = "Tutorial", menuName = "Scriptable Objects/Tutorial")]
public class Tutorial : ScriptableObject
{
    public bool RoomMoveTutorialCompleted = false;
    public bool FightTutorialCompleted = false;
    public bool ItemTutorialCompleted = false;
    public bool CanSkillTutorialCompleted = false;
    public bool CanMoveTutorialCompleted = false;
}
