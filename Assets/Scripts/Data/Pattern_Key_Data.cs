using UnityEngine;

public enum Pattern_Label
{
    None = 0,
    Attack = 1,
    Dash = 2,
    Wind = 3,
    Stone = 4
}


[CreateAssetMenu(fileName = "New ActionData", menuName = "Data/Pattern_Key")]
public class Pattern_Key_Data : ScriptableObject
{
    public Pattern_Label label;
    public string Name_key;
    public string Desc_key;
    public string Type_key;
}
