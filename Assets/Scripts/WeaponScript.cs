using System.Collections;

using Mirror;

using UnityEngine;

public class WeaponScript : NetworkBehaviour
{   
    public WeaponScriptObj WeaponScriptObj;

    public Camera PlayerCamera;

    private int _ammo;
    private float _timeToNextShot;
    private bool _reloading;

    private void Start()
    {
        _ammo = WeaponScriptObj.MaxAmmo;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        ShootDelay();
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
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

        _ammo--;
        _timeToNextShot = WeaponScriptObj.FireRate;
    }

    private void ShootDelay()
    {
        if (_timeToNextShot > 0)
        {
            _timeToNextShot -= Time.deltaTime;
        }
    }

    [Command]
    private void CmdServerDamageTarget(HealthScript healthScript)
    {
        healthScript.TakeDamageOnServer(WeaponScriptObj.Damage);
    }

    private IEnumerator Reload()
    {
        if (_reloading) yield return null;

        _reloading = true;
        yield return new WaitForSecondsRealtime(WeaponScriptObj.ReloadTime);
        _ammo = WeaponScriptObj.MaxAmmo;
        _reloading = false;
    }
}
