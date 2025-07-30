using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDeck", menuName = "TCG/Deck Data")]
public class DeckData : ScriptableObject
{
    public List<CardData> cards;
}
