using Photon.Pun;
using UnityEngine;
using System.Collections;

public class PlayerHitController : MonoBehaviourPun
{
    [SerializeField] private GameObject modelObject;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private float respawnDelay = 3.0f;

    private bool hasReceivedHit = false; // Flaga, aby zapobiec wielokrotnemu naliczaniu punktów

    private void Awake()
    {
        playerSpawner = FindAnyObjectByType<PlayerSpawner>();
    }

    [PunRPC]
    public void OnHitRPC(Vector3 throwedItemPosition, Vector3 throwedItemVelocity)
    {
        // Dodanie Rigidbody do modelu (jeśli jeszcze go nie ma) - robimy to w RPC, żeby było to synchronizowane
        Rigidbody rb = modelObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = modelObject.AddComponent<Rigidbody>();
        }

        // Wyłączenie kontroli ruchu gracza
        playerMovement.enabled = false;

        // Obliczanie kierunku trafienia
        Vector3 hitDirection = (modelObject.transform.position - throwedItemPosition).normalized;
        float hitForce = throwedItemVelocity.magnitude * 1.5f;

        // Wywołanie RPC tylko do dodania siły, aby synchronizować to z wszystkimi graczami
        photonView.RPC("ApplyForceToModel", RpcTarget.All, hitDirection, hitForce);

        // Rozpoczęcie procesu respawnu
        StartCoroutine(RespawnPlayer());
    }

    [PunRPC]
    public void ApplyForceToModel(Vector3 hitDirection, float hitForce)
    {
        // Sprawdzamy, który gracz wywołał RPC
        Debug.Log($"[ApplyForceToModel] RPC wywołane przez gracza: {PhotonNetwork.LocalPlayer.NickName}, " +
                  $"siła: {hitForce}, kierunek: {hitDirection}");

        Rigidbody rb = modelObject.GetComponent<Rigidbody>();

        // Debugowanie dodania Rigidbody
        if (rb == null)
        {
            Debug.Log($"[ApplyForceToModel] Dodano Rigidbody do modelu {modelObject.name}");
            rb = modelObject.AddComponent<Rigidbody>();
        }

        // Dodanie siły do modelu
        if (rb != null)
        {
            rb.AddForce(hitDirection * hitForce, ForceMode.Impulse);
            Debug.Log($"[ApplyForceToModel] Siła zastosowana na modelu {modelObject.name}, " +
                      $"kierunek: {hitDirection}, siła: {hitForce}");
        }
        else
        {
            Debug.LogWarning("[ApplyForceToModel] Nie udało się znaleźć Rigidbody, nie dodano siły.");
        }
    }

    public void OnHit(Rigidbody throwedItemRb, int points, Photon.Realtime.Player hitPlayer)
    {
        // Wywołanie RPC dla synchronizacji tylko siły na modelu
        if (!hasReceivedHit)
        {
            // Wywołanie RPC do dodania Rigidbody i siły
            photonView.RPC("OnHitRPC", RpcTarget.All, throwedItemRb.position, throwedItemRb.linearVelocity);

            hasReceivedHit = true;
        }

        // Naliczanie punktów tylko dla lokalnego gracza
        AddPoints(hitPlayer);
    }

    private void AddPoints(Photon.Realtime.Player hitPlayer)
    {
        // Naliczanie punktów tylko dla wybranego gracza
        if (hitPlayer.CustomProperties.ContainsKey("Score"))
        {
            int currentScore = (int)hitPlayer.CustomProperties["Score"];
            hitPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Score", currentScore + 10 } });
        }
        else
        {
            hitPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "Score", 10 } });
        }
    }


    private IEnumerator RespawnPlayer()
    {
        // Wyłączenie kolizji i renderowania modelu gracza
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        // Odczekanie określonego czasu przed respawnem
        yield return new WaitForSeconds(respawnDelay);

        // Przywrócenie kolizji i renderowania modelu gracza
        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
        modelObject.SetActive(true);
        var rb = modelObject.GetComponent<Rigidbody>();
        Destroy(rb);
        var chilCapsule = modelObject.GetComponentInChildren<CapsuleCollider>();
        modelObject.transform.localPosition = Vector3.zero;
        modelObject.transform.localRotation = Quaternion.identity;
        chilCapsule.gameObject.transform.localPosition = Vector3.zero;
        chilCapsule.gameObject.transform.localRotation = Quaternion.identity;

        // Przywrócenie kontroli ruchu gracza
        playerMovement.enabled = true;

        // Wywołanie metody respawnu w skrypcie PlayerSpawner
        if (playerSpawner != null)
        {
            playerSpawner.RespawnPlayer(gameObject);
        }
        else
        {
            Debug.LogError("PlayerSpawner nie jest przypisany!");
        }

        // Zresetowanie flagi po respawnie
        hasReceivedHit = false;
    }
}
