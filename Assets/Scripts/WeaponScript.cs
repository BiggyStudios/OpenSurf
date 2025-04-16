using UnityEngine;

public class WeaponScript : MonoBehaviour
{   
    public float Damage;
    public int MaxAmmo;

    private int _ammo;

    private void Start()
    {
        _ammo = MaxAmmo;
    }

    private void Update()
    {
        
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (_ammo <= 0) return;

        
    }
}
