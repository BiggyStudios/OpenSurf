using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class PlayerTimer : NetworkBehaviour
{
    [SerializeField] private TMP_Text _timerText;

    private void Update()
    {
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        PlayerManager.Instance.PlayerTime += Time.deltaTime;
        _timerText.text = PlayerManager.Instance.PlayerTime.ToString();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FinishPlatform"))
        {
            PlayerManager.Instance.Restart();
            Scoreboard.SetTime(PlayerManager.Instance.Username, PlayerManager.Instance.PlayerTime);
        }
    }
}
