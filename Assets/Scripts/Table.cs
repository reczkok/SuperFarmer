using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    [field: SerializeField]
    public Transform P1Spot { get; private set; }
    [field: SerializeField]
    public Transform P2Spot { get; private set; }
    [field: SerializeField]
    public Transform P3Spot { get; private set; }
    [field: SerializeField]
    public Transform P4Spot { get; private set; }
    public GameObject PlayerAreaPrefab;

    private void Awake()
    {
        P1Spot = GameObject.FindGameObjectWithTag("P1Slot").transform;
        P2Spot = GameObject.FindGameObjectWithTag("P2Slot").transform;
        P3Spot = GameObject.FindGameObjectWithTag("P3Slot").transform;
        P4Spot = GameObject.FindGameObjectWithTag("P4Slot").transform;
    }
    
    private Transform GetPlayerSpot(int playerNumber)
    {
        return playerNumber switch
        {
            0 => P1Spot,
            1 => P2Spot,
            2 => P3Spot,
            3 => P4Spot,
            _ => throw new ArgumentOutOfRangeException(nameof(playerNumber), playerNumber, null)
        };
    }
    
    public List<Player> CreatePlayers(List<string> names)
    {
        var players = new List<Player>();
        for (var i = 0; i < names.Count; i++)
        {
            var playerArea = Instantiate(PlayerAreaPrefab, GetPlayerSpot(i));
            var player = playerArea.GetComponent<Player>();
            player.PlayerName = names[i];
            player.PlayerNumber = i + 1;
            players.Add(player);
        }
        return players;
    }
}
