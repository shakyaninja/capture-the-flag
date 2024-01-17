using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    [SerializeField] private TMPro.TMP_Text timerText;
    [SerializeField] private TMPro.TMP_Text scoreText;
    [SerializeField] private GameObject ScoreList;
    [SerializeField] private GameObject ScorePanel;
    [SerializeField] private GameObject WaitingForPlayers;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject StartGameCountdownPanel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void UpdateTimerText()
    {
        timerText.SetText(GameManager.Instance.statsManager.GetTimer().ToString()) ;
    }

    private void UpdateScoreText()
    {
        //scoreText.SetText(GameManager.Instance.statsManager.GetScore().ToString());
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimerText();
        UpdateScoreText();
    }

    public void TriggerWaitingForPlayer()
    {
        MainMenu.SetActive(false);
        WaitingForPlayers.SetActive(true);
    }

    public void TriggerGameplay() {
        WaitingForPlayers.SetActive(false);
        StartGameCountdown();
    }

    public void StartGameCountdown()
    {
        StartGameCountdownPanel.SetActive(true);
    }
}
