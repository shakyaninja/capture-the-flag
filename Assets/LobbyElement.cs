using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyElement : MonoBehaviour
{
    public TMP_Text lobbyName;
    public TMP_Text lobbyCode;
    public TMP_Text maxPlayers;
    public Button JoinLobby;

    public void InitializeValues(string lobbyName, string lobbyCode, string maxPlayers)
    {
        this.lobbyName.SetText(lobbyName);
        this.lobbyCode.SetText(lobbyCode);
        this.maxPlayers.SetText(maxPlayers);
    }

    void Start()
    {
    
    }

    

    void Update()
    {
        
    }
}
