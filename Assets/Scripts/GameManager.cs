using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public Boolean isGameActive = false;
    /*public NetworkVariable<Player[]> players = new NetworkVariable<Player[]>(null,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);*/
    public Player[] players;
    public StatsManager statsManager = null;
    public UIManager uiManager = null;
    public Transform[] playerSpawnPositions;
    public Color[] playerSpawnColors;
    public GameObject PlayerPrefab;
    public Lobby activeLobby;

    public struct Player 
    {
        public ulong clientId;
        public string name;
        public int score;
    }

    /*public struct Player: INetworkSerializable
    {
        public ulong clientId;
        public string name;
        public int score;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref name);
            serializer.SerializeValue(ref score);
        }
    }*/

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(IsServer)
        {

        }
    }

    override public void OnNetworkSpawn()
    {
        if(IsServer)
        {
            //list all clients
            Debug.Log(NetworkManager.Singleton.ConnectedClients);
            RespawnFlag();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterClientAsPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log(serverRpcParams.Receive.SenderClientId);
        Player player = new Player();
        player.clientId = serverRpcParams.Receive.SenderClientId;
        player.name = serverRpcParams.Receive.SenderClientId + " : " + "Avatar ...";
        player.score = 0;
        //players.Value[players.Value.Length] = player;
        players[players.Length] = player;
        
        Debug.Log("player registered ...:"+player);
    }

    [ServerRpc(RequireOwnership = false)]

    public void UpdateScoreOfPlayerServerRpc(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log(serverRpcParams.Receive.SenderClientId);
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        /*for (int i = 0; i < players.Value.Length; i++)
        {
            if (players.Value[i].clientId == clientId)
            {
                players.Value[i].score = players.Value[i].score + 1;
            }
        }*/
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].clientId == clientId)
            {
                players[i].score = players[i].score + 1;
            }
        }

        Debug.Log("player score updated ...");
    }

    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        SpawnPlayer(NetworkManager.LocalClientId);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        SpawnPlayer(NetworkManager.LocalClientId);
    }

    public void RespawnFlag()
    {
        gameObject.GetComponent<SpawnFlag>().spawnFlag();
    }

    public void StartGame()
    {
        /*//check if available servers
        bool availableServers = false;
        if(availableServers)
        {
            //join them
            StartClient();
        }
        else
        {
            //create new server or host
            StartHost();
        }
        uiManager.TriggerWaitingForPlayer();*/
        //loop in players
        //Spawn each of them on new places
        foreach (var player in players)
        {
            SpawnPlayer(player.clientId);   
        }
        //spawn flag
    }

    public void SpawnPlayer(ulong clientId)
    {
        Debug.Log("clientId : " + clientId);
        int randomIndex = UnityEngine.Random.Range(0, playerSpawnPositions.Length - 1);
        Transform playerTransform = playerSpawnPositions[randomIndex];
        GameObject instantiatedPlayerPrefab = Instantiate(PlayerPrefab, playerTransform);
        instantiatedPlayerPrefab.GetComponent<Material>().color = playerSpawnColors[randomIndex];
        instantiatedPlayerPrefab.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    public void testBtnClick()
    {
        Debug.Log("btn clicked asd as");
    }
}
