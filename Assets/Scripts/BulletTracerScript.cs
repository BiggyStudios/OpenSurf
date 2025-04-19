using Mirror;
using UnityEngine;

public class BulletTracerScript : NetworkBehaviour
{
    public float TracerSpeed;
    public Vector3 ShootDirection;

    private void Update()
    {
        transform.position += ShootDirection * TracerSpeed * Time.deltaTime;
    }
}
