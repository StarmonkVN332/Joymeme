using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckDatabase", menuName = "Game/Deck Database")]
public class DeckDatabase : ScriptableObject
{
    public List<DeckData> allDecks;

    public DeckData GetDeckByName(string deckName)
    {
        return allDecks.Find(deck => deck.name == deckName);
    }
}