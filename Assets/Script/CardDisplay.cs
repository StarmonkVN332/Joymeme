using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public CardData cardData;

    public TextMeshPro attackText;
    public TextMeshPro healthText;
    public TextMeshPro manaText;
    public MeshRenderer artworkRenderer;

    public void Load(CardData data)
    {
        cardData = data;

        attackText.text = data.cardType == CardType.Unit ? data.attack.ToString() : "";
        healthText.text = data.cardType == CardType.Unit ? data.health.ToString() : "";
        manaText.text = data.manaCost.ToString();

        if (data.image != null)
        {
            Material mat = new Material(artworkRenderer.material);
            mat.mainTexture = data.image.texture;
            artworkRenderer.material = mat;
        }
    }

    private void OnMouseDown()
    {
        if (CardInteractionManager.Instance.CanSelect(this))
            CardInteractionManager.Instance.Select(this);
    }
}
