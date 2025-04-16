using P90brush;

using UnityEngine;

public class Bouncepad : MonoBehaviour
{
    [SerializeField] private Vector3 _launchForce;

    public void LaunchPlayer(PlayerLogic playerLogic)
    {        
        Vector3 launchVectorX = playerLogic.transform.right * _launchForce.x;
        Vector3 launchVectorY = playerLogic.transform.up * _launchForce.y;
        Vector3 launchVectorZ = playerLogic.transform.forward * _launchForce.z;

        Vector3 launchVector = launchVectorX + launchVectorY + launchVectorZ;

        playerLogic.PlayerData.Velocity += launchVector;
    }
}
