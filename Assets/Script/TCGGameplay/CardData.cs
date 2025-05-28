using UnityEngine;

[CreateAssetMenu(menuName = "Card Data")]
public class CardData : ScriptableObject
{
    public int cardId;  // unique identifier
    public string cardName;
    public string description;
    public string abilityDescription;
    public Sprite cardImage;
    public int manaCost;
    public int health;
    public int attack;
    public CardType cardType;
    public GameObject unitPrefab;
}


public enum CardType { Unit, Spell }
