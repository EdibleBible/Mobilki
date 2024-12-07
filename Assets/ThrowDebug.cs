using UnityEngine;

public class ThrowDebug : MonoBehaviour
{
    [SerializeField] private ThrowableItem itemToThrow;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            itemToThrow.ThrowItem(1000, itemToThrow.transform.right);
        }
    }
}
