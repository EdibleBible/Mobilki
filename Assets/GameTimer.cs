using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviourPunCallbacks
{
    public float GameDuration = 300f; // Czas trwania gry w sekundach
    private double StartTime;
    private bool gameEnded = false;

    [SerializeField] private PlayerPropertiesManager playerPropertiesManager;
    [SerializeField] private GameObject hostPlayerMenu;
    [SerializeField] private GameObject normalPlayerMenu;
    [SerializeField] private TextMeshProUGUI winInformationText;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartTime = PhotonNetwork.Time; // Zapisujemy czas początkowy gry
            // Ustawiamy StartTime w CustomProperties pokoju tylko raz, na początku
            Hashtable roomProperties = new Hashtable { { "StartTime", StartTime } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
        else
        {
            // Gracze dołączający odczytują StartTime z CustomProperties
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("StartTime", out object startTimeObj))
            {
                StartTime = (double)startTimeObj;
            }
        }
    }

    void LateUpdate()
    {
        if (!gameEnded)
        {
            double elapsedTime = PhotonNetwork.Time - StartTime;

            if (elapsedTime >= GameDuration)
            {
                EndGame(); // Zakończenie gry
            }
            else
            {
                winInformationText.gameObject.SetActive(false);
            }
        }
    }

    private void EndGame()
    {
        Dictionary<Player, int> playersScore = new Dictionary<Player, int>();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            playersScore.Add(player, playerPropertiesManager.GetPlayerScore(player));
        }

        gameEnded = true;

        winInformationText.gameObject.SetActive(true);

        // Znajdź maksymalny wynik
        int maxScore = playersScore.Values.Max();

        // Znajdź wszystkich graczy z maksymalnym wynikiem
        var playersWithMaxScore = playersScore.Where(pair => pair.Value == maxScore).ToList();

        // Sprawdź, czy jest remis
        if (playersWithMaxScore.Count > 1)
        {
            winInformationText.text = $"Draw! you have {maxScore} points!";
        }
        else
        {
            var winner = playersWithMaxScore.First();
            if (winner.Key == PhotonNetwork.LocalPlayer)
            {
                winInformationText.text = $"You win! you have {winner.Value} points!";
            }
            else
            {
                winInformationText.text = $"You lose! Player {winner.Key.NickName} wins!";
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // Host wyświetla menu po zakończeniu czasu
            ShowHostMenu();
            photonView.RPC("DisplayWaitMessage", RpcTarget.Others);
        }
    }

    // Funkcja wyświetlająca menu hosta
    private void ShowHostMenu()
    {
        // Tutaj wywołaj metodę odpowiedzialną za wyświetlenie menu resetu/wyjścia u hosta
        // Możesz tu uruchomić UI (np. Canvas z menu)
        hostPlayerMenu.SetActive(true);
    }

    public void ResetGameTimer()
    {
        photonView.RPC("CloseMenu", RpcTarget.All);

        photonView.RPC("ResetTime", RpcTarget.All);

        photonView.RPC("ResetTimeUI", RpcTarget.All);
    }

    [PunRPC]
    private void DisplayWaitMessage()
    {
        normalPlayerMenu.SetActive(true);
    }

    [PunRPC]
    private void CloseMenu()
    {
        winInformationText.gameObject.SetActive(false);

        if (hostPlayerMenu != null)
        {
            hostPlayerMenu.SetActive(false);
        }

        if (normalPlayerMenu != null)
        {
            normalPlayerMenu.SetActive(false);
        }
    }

    [PunRPC]
    private void ResetTime()
    {
        gameEnded = false;
        winInformationText.gameObject.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            StartTime = PhotonNetwork.Time; // Zapisujemy czas początkowy gry
            // Ustawiamy StartTime w CustomProperties pokoju tylko raz, na początku
            Hashtable roomProperties = new Hashtable { { "StartTime", StartTime } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
        else
        {
            StartTime = PhotonNetwork.Time;
        }
    }
}