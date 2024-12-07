using UnityEngine;

public class PlayerPickUpObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pickupedObject;
    [SerializeField] private PlayerInputReader inputReader;

    [Header("Values")]
    [SerializeField] private Transform pickupTransform;
    [SerializeField] public Transform HoldItemTransform;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask pickupLayer;

    [Header("Charge Settings")]
    [SerializeField, Range(0, 5f)] private float maxChargeTime = 3f; // Czas, po którym osiągamy maksymalną moc
    [SerializeField] private float throwPower = 4f;
    [SerializeField] private AnimationCurve chargeCurve; // Krzywa definiująca wzrost mocy
    private float currentChargeTime = 0f; // Aktualny czas ładowania
    private bool isCharging = false;

    private void OnEnable()
    {
        inputReader.OnInteractPerformed += Interact;
        inputReader.OnChargeStarted += StartCharge;
        inputReader.OnChargeCanceled += StopCharge;
    }

    private void OnDisable()
    {
        inputReader.OnInteractPerformed -= Interact;
        inputReader.OnChargeStarted -= StartCharge;
        inputReader.OnChargeCanceled -= StopCharge;
    }

    private void Interact()
    {
        // Zmiana na BoxCast
        if (!Physics.BoxCast(pickupTransform.position, Vector3.one * 0.5f, pickupTransform.forward, out RaycastHit hitObj, pickupTransform.rotation, pickupRange, pickupLayer))
            return;

        if (pickupedObject == null)
        {
            if (!hitObj.collider.TryGetComponent(out IPickable pickable))
                return;

            if (pickable.IsDisable)
                return;

            pickupedObject = pickable.PickUpItem(HoldItemTransform, transform);
        }
        else
        {
            if (!pickupedObject.TryGetComponent(out IPickable pickable))
                return;

            pickable.DropItem(ref pickupedObject);
        }
    }
    private void StartCharge()
    {
        Debug.Log("Start Charging");
        isCharging = true;
        currentChargeTime = 0f;
    }
    private void StopCharge()
    {
        isCharging = false;
        Debug.Log("Stop Charging with Power wthoutItemInHands");
        if (pickupedObject == null)
            return;

        float chargePower = GetChargePower();
        Debug.Log($"Stop Charging with Power: {chargePower}");
        if (pickupedObject.TryGetComponent<IThrowableItem>(out var item))
        {
            item.ThrowItemWithDrop(ref pickupedObject, throwPower * chargePower, pickupTransform.forward);
        }
    }
    private void Update()
    {
        // Debugowanie BoxCast
        DebugBoxCast();

        // Zmiana na BoxCast
        if (Physics.BoxCast(pickupTransform.position, Vector3.one * 0.5f, pickupTransform.forward, out RaycastHit hitObj, pickupTransform.rotation, pickupRange, pickupLayer))
        {
            if (hitObj.collider.TryGetComponent(out IPickable item))
            {
                item.HighLight();
            }
        }

        if (isCharging)
        {
            // Aktualizuj czas ładowania
            currentChargeTime += Time.deltaTime;

            // Ogranicz do maksymalnego czasu ładowania
            currentChargeTime = Mathf.Min(currentChargeTime, maxChargeTime);

            // Opcjonalnie: Wyświetl aktualną moc w debugu
            float chargePower = GetChargePower();
            Debug.Log($"Charging... Power: {chargePower}");
        }
    }
    private void DebugBoxCast()
    {
        // Rysowanie BoxCast w edytorze
        Vector3 startPosition = pickupTransform.position;
        Vector3 direction = pickupTransform.forward;

        // Rozmiar "boxa" (powinno odpowiadać wymiarom gracza, który jest kapsułą)
        Vector3 boxSize = new Vector3(0.5f, 2f, 0.5f); // Dopasuj do wielkości gracza

        // Narysowanie boxa
        Debug.DrawRay(startPosition, direction * pickupRange, Color.red); // Promień

        // Opcjonalnie: Można narysować box, aby pokazać jego zasięg
        Debug.DrawRay(startPosition, Vector3.up * 0.1f, Color.blue); // Mała niebieska linia, wskazująca początek promienia
    }
    private float GetChargePower()
    {
        // Oblicz aktualną moc na podstawie krzywej
        float normalizedTime = currentChargeTime / maxChargeTime; // Normalizacja czasu ładowania (0 do 1)
        return chargeCurve.Evaluate(normalizedTime); // Pobierz wartość z krzywej
    }
}
