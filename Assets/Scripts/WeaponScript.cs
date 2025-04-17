using Mirror;

using UnityEngine;

public class WeaponScript : NetworkBehaviour
{   
    public WeaponScriptObj WeaponScriptObj;

    public Camera PlayerCamera;

    private int _ammo;

    private void Start()
    {
        _ammo = WeaponScriptObj.MaxAmmo;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (_ammo <= 0) return;
        
        Ray ray = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);
        RaycastHit hit;

        Debug.Log("Shot");

        if (Physics.Raycast(ray, out hit, WeaponScriptObj.WeaponRange))
        {
            HealthScript targetHealth = hit.collider.GetComponent<HealthScript>();
            if (targetHealth != null)
            {
                Debug.Log("Shot hit");
                //CmdServerDamageTarget(targetHealth.netId, Damage);
                CmdServerDamageTarget(targetHealth);
            }
        }
    }

    [Command]
    private void CmdServerDamageTarget(HealthScript healthScript)
    {
        Debug.Log("Called server damage RPC");

        /*
        if (NetworkServer.spawned.TryGetValue(targetNetID, out NetworkIdentity targetIdentity))
        {

            Debug.Log("Called net-server damage on netID of player");
            HealthScript targetHealthScript = targetIdentity.GetComponent<HealthScript>();

            if (targetHealthScript != null)
            {
                Debug.Log("Actually damaged the player");
                targetHealthScript.TakeDamageOnServer(damageAmount);
            }
        }
        */

        healthScript.TakeDamageOnServer(WeaponScriptObj.Damage);
    }
}
