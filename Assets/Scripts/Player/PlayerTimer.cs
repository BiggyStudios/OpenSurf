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
        if (!base.IsOwner)
            return;

        UpdateTimer();
        CheckStart();
        CheckFinish();
    }

    private void UpdateTimer()
    {
        if (!PlayerManager.Instance.TimerActive)
        {
            _timerText.text = FormatTime(PlayerManager.Instance.PlayerTime);
            return;
        }

        PlayerManager.Instance.PlayerTime += Time.deltaTime;
        _timerText.text = FormatTime(PlayerManager.Instance.PlayerTime);
    }

    private void CheckFinish()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 2f))
        {
            if (hit.transform.CompareTag("FinishPlatform"))
            {
                PlayerManager.Instance.TimerActive = false;

                Scoreboard.Instance.UpdatePlayerTime(PlayerManager.Instance.OwnerId.ToString(), PlayerManager.Instance.PlayerTime);
                PlayerManager.Instance.Restart();
            }
        }

    }

    private void CheckStart()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, -transform.up, out hit, 5f);

        if (hit.transform == null)
        {
            Debug.Log("Yes");
            PlayerManager.Instance.TimerActive = true;
        }
    }

    private string FormatTime(float time)
    {
        int mins = (int)(time / 60f);
        int secs = (int)(time % 60f);
        int milisecs = (int)((time * 100f) % 100f);

        return $"{mins:00}:{secs:00}.{milisecs:00}";
    }
}
