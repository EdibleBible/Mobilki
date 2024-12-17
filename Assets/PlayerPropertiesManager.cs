using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;

public class PlayerPropertiesManager : MonoBehaviourPunCallbacks
{
    // Ustawienie Custom Properties dla lokalnego gracza
    public void SetPlayerProperty(string key, object value)
    {
        Hashtable customProps = new Hashtable
        {
            { key, value }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);
    }

    // Pobranie Custom Property od dowolnego gracza
    public object GetPlayerProperty(Player player, string key)
    {
        if (player.CustomProperties.TryGetValue(key, out object value))
        {
            return value;
        }
        return null; // Zwraca null, jeśli klucz nie istnieje
    }

    // Przykład: ustawienie punktów
    public void SetPlayerScore(int score)
    {
        SetPlayerProperty("Score", score);
    }

    // Przykład: pobranie punktów
    public int GetPlayerScore(Player player)
    {
        object score = GetPlayerProperty(player, "Score");
        return score != null ? (int)score : 0; // Domyślna wartość to 0
    }

    // Obsługa zmian właściwości
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Score"))
        {
            int newScore = (int)changedProps["Score"];
            Debug.Log($"{targetPlayer.NickName} ma teraz {newScore} punktów!");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Pobierz aktualny wynik lokalnego gracza
            int currentScore = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Score")
                ? (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"]
                : 0;

            // Dodaj punkty
            int newScore = currentScore + 10;

            // Ustaw nowy wynik w Custom Properties
            ExitGames.Client.Photon.Hashtable customProps = new ExitGames.Client.Photon.Hashtable
        {
            { "Score", newScore }
        };
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProps);

            Debug.Log($"Dodano punkty! Nowy wynik lokalnego gracza: {newScore}");
        }
    }
}