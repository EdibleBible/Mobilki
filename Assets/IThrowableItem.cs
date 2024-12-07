using UnityEngine;

public interface IThrowableItem
{
    public void ThrowItemWithDrop(ref GameObject ItemInPlayerHand,float power, Vector3 direction);
    public void ThrowItem(float power, Vector3 direction);
    public void CheckFlightState();
    public void OnItemHit();
    public float flightCheckThreshold { get; set; }
    public float flightCheckDelay { get; set; }

}

public interface IPickable
{
    public bool IsDisable { get; set; }
    public GameObject PickUpItem(Transform holdParent, Transform playerParent);
    public void DropItem(ref GameObject ItemInPlayerHand);
    public void HighLight();
}