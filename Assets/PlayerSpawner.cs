using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerUIItemPrefab; // Prefab dla UI gracza
    [SerializeField] private Transform playerListParent;  // Rodzic dla listy graczy w UI
    [SerializeField] private PlayerPropertiesManager playerPropertiesManager;
    private List<GameObject> globalPlayerList = new List<GameObject>();
    
    private Dictionary<string, GameObject> playerItems = new Dictionary<string, GameObject>();

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            FirstPlayerSpawn();
            UpdatePlayerListUI();
        }
    }

    public void FirstPlayerSpawn()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Prefab gracza nie został przypisany!");
            return;
        }

        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity);

        if (player == null)
        {
            Debug.LogError("Nie udało się zespawnować gracza!");
            return;
        }

        // Nadanie unikalnego UserId
        PhotonView photonView = player.GetComponent<PhotonView>();
        if (photonView != null)
        {
            // Generowanie unikalnego UserId (np. używając GUID)
            string uniqueId = System.Guid.NewGuid().ToString();
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "UserId", uniqueId } });

            // Ustawianie UserId w PhotonPlayer
            photonView.Owner.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "UserId", uniqueId } });
            Debug.Log($"Ustawiono UserId dla gracza {PhotonNetwork.LocalPlayer.NickName}: {uniqueId}");
        }

        AddPlayerToGlobalList(player);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(-5f, 5f);
        float randomZ = Random.Range(-5f, 5f);
        return new Vector3(randomX, 1.5f, randomZ);
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

        // Dodaj domyślne punkty dla lokalnego gracza
        if (photonView.IsMine)
        {
            ExitGames.Client.Photon.Hashtable initialProps = new ExitGames.Client.Photon.Hashtable
            {
                { "Score", 0 } // Domyślna liczba punktów
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);
        }
    }

    [PunRPC]
    private void RPC_AddPlayerToGlobalList(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view == null)
        {
            Debug.LogError($"Nie znaleziono PhotonView o ViewID {viewID}!");
            return;
        }

        GameObject player = view.gameObject;
        globalPlayerList.Add(player);
        Debug.Log($"Dodano gracza do globalnej listy: {player.name}");
        UpdatePlayerListUI();
    }

    public void RespawnPlayer(GameObject player)
    {
        Vector3 respawnPosition = GetRandomSpawnPosition();
        player.transform.position = respawnPosition;
        player.SetActive(true);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Score"))
        {
            UpdatePlayerListUI();
        }
    }

    private void UpdatePlayerListUI()
    {
        // Wyczyść istniejącą listę w UI
        foreach (Transform child in playerListParent)
        {
            Destroy(child.gameObject);
        }
        playerItems.Clear();

        // Przejdź przez listę graczy Photon
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject item = Instantiate(playerUIItemPrefab, playerListParent);
            TextMeshProUGUI itemText = item.transform.Find("Text Player Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = item.transform.Find("Text Player Score").GetComponent<TextMeshProUGUI>();

            // Ustawienie nazwy gracza i punktów
            string playerName = player.NickName;
            int playerScore = player.CustomProperties.ContainsKey("Score")
                ? (int)player.CustomProperties["Score"]
                : 0;

            itemText.text = $"{playerName}";
            scoreText.text = $"Score: {playerScore}";

            // Dodanie do słownika graczy
            string key = !string.IsNullOrEmpty(player.UserId) ? player.UserId : player.ActorNumber.ToString();
            playerItems[key] = item;
        }
    }


    public void UpdatePlayerScore(int newScore)
    {
        ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
        {
            { "Score", newScore }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }
}
