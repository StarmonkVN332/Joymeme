using UnityEngine;

public class LaneSlot : MonoBehaviour
{
    private void OnMouseDown()
    {
        CardInteractionManager.Instance.TryPlaceCard(this.transform);
    }
}
