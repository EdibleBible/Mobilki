using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    private List<GameObject> globalPlayerList = new List<GameObject>();

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            FirstPlayerSpawn();
        }
    }

    public void FirstPlayerSpawn()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Prefab gracza nie został przypisany!");
            return;
        }

        float randomX = Random.Range(-5f, 5f);
        float randomZ = Random.Range(-5f, 5f);
        Vector3 spawnPosition = new Vector3(randomX, 1.5f, randomZ);

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        if (player == null)
        {
            Debug.LogError("Nie udało się zespawnować gracza!");
            return;
        }

        AddPlayerToGlobalList(player);
    }

    private void AddPlayerToGlobalList(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("Gracz jest null! Nie można dodać do listy.");
            return;
        }

        PhotonView photonView = player.GetComponent<PhotonView>();
        if (photonView == null)
        {
            Debug.LogError("PhotonView nie istnieje na prefabrykacie gracza!");
            return;
        }

        globalPlayerList.Add(player);
        photonView.RPC("RPC_AddPlayerToGlobalList", RpcTarget.OthersBuffered, photonView.ViewID);
    }

    [PunRPC]
    private void RPC_AddPlayerToGlobalList(int viewID)
    {
        Debug.Log($"Otrzymano RPC_AddPlayerToGlobalList z ViewID: {viewID}");

        PhotonView view = PhotonView.Find(viewID);
        if (view == null)
        {
            Debug.LogError($"Nie znaleziono PhotonView o ViewID {viewID}!");
            return;
        }

        GameObject player = view.gameObject;
        globalPlayerList.Add(player);
        Debug.Log($"Dodano gracza do globalnej listy: {player.name}");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Zawartość globalnej listy graczy:");
            foreach (var player in globalPlayerList)
            {
                Debug.Log(player?.name ?? "null");
            }
        }
    }
}
