using System.Collections;

using Mirror;

using TMPro;

using UnityEngine;

public class WeaponScript : NetworkBehaviour
{   
    public WeaponScriptObj WeaponScriptObj;

    public BulletTracerScript BulletTracer;
    public Transform GunPoint;
    public Camera PlayerCamera;
    public TMP_Text AmmoText;

    private WeaponRecoilScript _weaponRecoilScript;

    private int _ammo;
    private float _timeToNextShot;
    private bool _reloading;
    private bool _aiming;

    private Vector3 _initialPosition;

    private void Start()
    {
        _weaponRecoilScript = GetComponent<WeaponRecoilScript>();
        _initialPosition = transform.localPosition;

        _ammo = WeaponScriptObj.MaxAmmo;
        AmmoText.text = new string("Ammo:" + _ammo);
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        ShootDelay();
        HandleInput();
        HandleAim();
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

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            _aiming = true;
        }

        else if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            _aiming = false;
        }
    }

    private void HandleAim()
    {
        if (_aiming)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, WeaponScriptObj.ADSPosition, Time.deltaTime * WeaponScriptObj.AimSpeed);
        }

        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition, Time.deltaTime * WeaponScriptObj.AimSpeed);
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

        SpawnTracerClient();
        CmdSpawnTracerServer();

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
    
    private void SpawnTracerClient()
    {
        Vector3 shootDirection = GetShootDirection();

        if (!isServer)
        {
            var tracer =
                Instantiate(BulletTracer.gameObject, GunPoint.transform.position, GunPoint.transform.rotation);

            tracer.GetComponent<BulletTracerScript>().ShootDirection = shootDirection;
        }

        if (isServer)
        {
            SpawnTracerClients();
        }
    }

    [Command]
    private void CmdSpawnTracerServer()
    {
        Vector3 shootDirection = GetShootDirection();

        var tracer =
            Instantiate(BulletTracer.gameObject, GunPoint.transform.position, GunPoint.transform.rotation);

        tracer.GetComponent<BulletTracerScript>().ShootDirection = shootDirection;

        //NetworkServer.Spawn(tracer);

        Destroy(tracer, 5f);
    }

    [ClientRpc]
    private void SpawnTracerClients()
    {
        Vector3 shootDirection = GetShootDirection();

        var tracer =
            Instantiate(BulletTracer.gameObject, GunPoint.transform.position, GunPoint.transform.rotation);

        tracer.GetComponent<BulletTracerScript>().ShootDirection = shootDirection;

        Destroy(tracer, 5f);
    }

    private Vector3 GetShootDirection()
    {
        Ray ray = PlayerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, WeaponScriptObj.WeaponRange))
        {
            targetPoint = hit.point;
        }

        else
        {
            targetPoint = ray.origin + ray.direction * WeaponScriptObj.WeaponRange;
        }

        Vector3 shootDirection = (targetPoint - GunPoint.position).normalized;

        return shootDirection;
    }

    private IEnumerator Reload()
    {
        if (_ammo >= 30) yield break;
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
