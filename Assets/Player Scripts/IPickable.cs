using Fusion;
using UnityEngine;

public interface IPickable
{
    public bool IsDisable { get; set; }
    public NetworkObject PickUpItem(Transform holdParent, Transform playerParent);
    public void DropItem(ref NetworkObject ItemInPlayerHand);
    public void HighLight();
}