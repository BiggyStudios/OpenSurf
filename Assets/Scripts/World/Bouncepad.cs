using P90brush;

using UnityEngine;

public class Bouncepad : MonoBehaviour
{
    [SerializeField] private Vector3 _launchForce;

    public void LaunchPlayer(PlayerLogic playerLogic)
    {
        playerLogic.PlayerData.Velocity += _launchForce;
    }
}
