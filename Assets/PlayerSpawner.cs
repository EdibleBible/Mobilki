using System;
using System.Collections.Generic;
using System.Data;
using Unity.Mathematics;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> playerList;

    private void Start()
    {
        
    }

    public void FirstaplayerSpawn()
    {
        var playerHodler = new GameObject($"PlayerHolder {playerList.Count + 1}");
        float randomX = UnityEngine.Random.Range(0, 5);
        float randomY = UnityEngine.Random.Range(0, 5);

        Instantiate(playerPrefab, new Vector3(randomX, 1.5f, randomY), quaternion.identity);
    }
}
