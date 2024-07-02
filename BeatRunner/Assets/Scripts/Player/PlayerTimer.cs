using System;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using UnityEngine;

public class PlayerTimer : NetworkBehaviour
{
    [SerializeField] private LayerMask _finishLayer;
    [SerializeField] private TMP_Text _timerText;

    private void Update()
    {
        UpdateTimer();
        CheckFinish();
    }

    private void UpdateTimer()
    {
        PlayerManager.Instance.PlayerTime += Time.deltaTime;
        _timerText.text = PlayerManager.Instance.PlayerTime.ToString();
    }

    private void CheckFinish()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 2f))
        {
            if (hit.transform.CompareTag("FinishPlatform"))
            {
                PlayerManager.Instance.Restart();
                Scoreboard.SetTime(PlayerManager.Instance.Username, PlayerManager.Instance.PlayerTime);
            }
        }
            
    }
}
