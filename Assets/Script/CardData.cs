using UnityEngine;

public enum CardType { Unit, Spell }

[CreateAssetMenu(menuName = "TCG/Card Data")]
public class CardData : ScriptableObject
{
    public string cardName;
    [TextArea] public string description;
    [TextArea] public string abilityDescription;
    public int manaCost;
    public int attack;
    public int health;
    public Sprite image;
    public CardType cardType;
}
