using UnityEngine;

[CreateAssetMenu(fileName = "WeaponScriptObj", menuName = "NewWeaponScriptObj")]
public class WeaponScriptObj : ScriptableObject
{
    public string WeaponName;
    public float Damage;
    public float WeaponRange;
    public float FireRate;
    public float ReloadTime;
    public float DamageFallOff;
    public Vector3 ADSPosition;
    public float AimSpeed;
    public int MaxAmmo;

    [Header("Recoil")]
    public Vector3 PositionRecoilAmount = new Vector3(0, 0, -0.1f);
    public Vector3 RotationRecoilAmount = new Vector3(-10f, 4f, 0f);
    public float PositionReturnSpeed = 10f;
    public float RotationReturnSpeed = 12f;
    public float SnapMultiplier = 60f;
}
