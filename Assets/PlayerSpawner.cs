using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerUIItemPrefab; // Prefab dla UI gracza
    [SerializeField] private Transform playerListParent;  // Rodzic dla listy graczy w UI
    [SerializeField] private PlayerPropertiesManager playerPropertiesManager;
    [SerializeField] private GameObject normalMenu;
    [SerializeField] private GameObject hostMenu;
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private GameObject resetGameButton;
    [SerializeField] private GameTimer gameTimer;
    [SerializeField] private TextMeshProUGUI playerCountText; // Tekst wyświetlający liczbę graczy
    private List<GameObject> globalPlayerList = new List<GameObject>();
    
    private Dictionary<string, GameObject> playerItems = new Dictionary<string, GameObject>();

    private void Start()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            hostMenu.SetActive(true);
            startGameButton.SetActive(true);
            resetGameButton.SetActive(false);
            UpdatePlayerCountUI();
        }
        else
        {
            normalMenu.SetActive(true);
        }
    }
    
    private void UpdatePlayerCountUI()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount; // Liczba aktualnych graczy
            int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;     // Maksymalna liczba graczy

            playerCountText.text = $"{currentPlayers}/{maxPlayers} Players in Lobby";
        }
        else
        {
            playerCountText.text = "0/0 Players in Lobby";
        }
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"Player {newPlayer.NickName} joined the room.");
        UpdatePlayerCountUI();
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log($"Player {otherPlayer.NickName} left the room.");
        UpdatePlayerCountUI();
    }

    public void StartGame()
    {
        // Upewnij się, że tylko host może uruchomić grę
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("Tylko host może uruchomić grę!");
            return;
        }

        Debug.Log("Gra została uruchomiona przez hosta!");

        // Wywołanie RPC na wszystkich klientach, aby każdy gracz zespawnował swojego gracza
        startGameButton.SetActive(false);
        resetGameButton.SetActive(true);
        photonView.RPC("FirstPlayerSpawn", RpcTarget.All);
        gameTimer.StartGame();
    }

    [PunRPC]
    public void FirstPlayerSpawn()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Prefab gracza nie został przypisany!");
            return;
        }

        // Losowe miejsce startowe dla gracza
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
            string uniqueId = System.Guid.NewGuid().ToString();
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "UserId", uniqueId } });

            photonView.Owner.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "UserId", uniqueId } });
            Debug.Log($"Ustawiono UserId dla gracza {PhotonNetwork.LocalPlayer.NickName}: {uniqueId}");
        }
        
        hostMenu.SetActive(false);
        normalMenu.SetActive(false);
        
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
