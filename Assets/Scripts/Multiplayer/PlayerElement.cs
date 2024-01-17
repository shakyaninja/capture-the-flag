using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerElement : MonoBehaviour
{
    public TMP_Text playerName;
    public TMP_Text playerId;
    public TMP_Text status;
    public Button KickPlayer;

    public void InitializeValues(string playerName, string playerId, string status)
    {
        this.playerName.SetText(playerName);
        this.playerId.SetText(playerId);
        this.status.SetText(status);
        if (status == "member")
        {
            KickPlayer.onClick.AddListener(delegate { LobbyManager.Instance.KickPlayer(playerId); });
        }
        else
        {
            KickPlayer.gameObject.SetActive(false);
        }
    }

    void Start()
    {
    
    }

    

    void Update()
    {
        
    }
}
