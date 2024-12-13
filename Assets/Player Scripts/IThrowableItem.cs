using Fusion;
using UnityEngine;

public interface IThrowableItem
{
    public void ThrowItemWithDrop(ref NetworkObject ItemInPlayerHand, float power, Vector3 direction);
    public void ThrowItem(float power, Vector3 direction);
    public void CheckFlightState();
    public void OnItemHit();
    public float flightCheckThreshold { get; set; }
    public float flightCheckDelay { get; set; }
}