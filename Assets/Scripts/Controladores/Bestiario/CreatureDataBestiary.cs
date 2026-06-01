using UnityEngine;

[CreateAssetMenu(fileName = "NewCreature", menuName = "Bestiary/Creature")]
public class CreatureDataBestiary : ScriptableObject
{
    public string creatureName;
    public Sprite portrait;
    [TextArea(3, 6)]
    public string description;
    public string habitat;
    public int health;
    public int damage;
}