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

        if (Physics.Raycast(ray, out hit, WeaponScriptObj.WeaponRange))
        {
            HealthScript targetHealth = hit.collider.GetComponent<HealthScript>();
            if (targetHealth != null)
            {
                CmdServerDamageTarget(targetHealth);
            }
        }
    }

    [Command]
    private void CmdServerDamageTarget(HealthScript healthScript)
    {
        healthScript.TakeDamageOnServer(WeaponScriptObj.Damage);
    }
}
