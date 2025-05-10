using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "TCG/CardData")]
public class CardData : ScriptableObject
{
    public string cardName;
    public Sprite art;
    public int manaCost;
    public int attack;
    public int health;
    [TextArea] public string description;
    public CardType type; // enum Unit or Spell
}

public enum CardType { Unit, Spell }
