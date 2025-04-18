using TMPro;
using UnityEngine;
using Mirror;

public class PlayerTimer : NetworkBehaviour
{
    [SerializeField] private LayerMask _finishLayer;

    private void Update()
    {
        if (!base.isLocalPlayer) return;

        UpdateTimer();
        CheckStart();
        CheckFinish();
    }

    private void UpdateTimer()
    {
        if (!PlayerManager.Instance.TimerActive)
        {
            return;
        }

        PlayerManager.Instance.PlayerTime += Time.deltaTime;
    }

    private void CheckFinish()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, 2f))
        {
            if (hit.transform.CompareTag("FinishPlatform"))
            {
                PlayerManager.Instance.TimerActive = false;

                //Scoreboard.Instance.UpdatePlayerTime(PlayerManager.Instance.netId.ToString(), PlayerManager.Instance.PlayerTime);
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
