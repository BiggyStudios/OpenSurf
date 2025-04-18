using System.Collections;

using Mirror;

using TMPro;

using UnityEngine;

public class WeaponScript : NetworkBehaviour
{   
    public WeaponScriptObj WeaponScriptObj;

    public Camera PlayerCamera;
    public TMP_Text AmmoText;

    private WeaponRecoilScript _weaponRecoilScript;

    private int _ammo;
    private float _timeToNextShot;
    private bool _reloading;

    private void Start()
    {
        _weaponRecoilScript = GetComponent<WeaponRecoilScript>();

        _ammo = WeaponScriptObj.MaxAmmo;
        AmmoText.text = new string("Ammo:" + _ammo);
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        ShootDelay();
        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetKey(KeyCode.Mouse0))
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
        if (_reloading) return;
        if (_ammo <= 0) return;
        if (_timeToNextShot > 0) return;
        
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
        _weaponRecoilScript.TriggerRecoil();
        AmmoText.text = new string("Ammo:" + _ammo);
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
        healthScript.UpdateHealthText();
    }

    private IEnumerator Reload()
    {
        if (_reloading) yield break;

        _reloading = true;

        float timer = 0f;
        float duration = WeaponScriptObj.ReloadTime;

        Quaternion startRotation = transform.localRotation;
        Vector3 normalizedSpinAxis = -Vector3.right.normalized;
        
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            
            float progress = Mathf.Clamp01(timer / duration);
            float currentAngle = Mathf.Lerp(0f, 720f, progress);

            Quaternion targetRotation = startRotation * Quaternion.AngleAxis(currentAngle, normalizedSpinAxis);
            transform.localRotation = targetRotation;

            yield return null;
        }

        transform.localRotation = startRotation * Quaternion.AngleAxis(720f, normalizedSpinAxis);

        _ammo = WeaponScriptObj.MaxAmmo;
        AmmoText.text = new string("Ammo:" + _ammo);
        _reloading = false;
    }
}
