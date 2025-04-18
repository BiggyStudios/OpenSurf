using Mirror;

using P90brush;

using UnityEngine;

public class WeaponSwayScript : NetworkBehaviour
{
    public PlayerLogic PlayerLogic;

    public float MouseSwayAmount;
    public float MaxMouseSwayAmount;
    public float MouseSmoothAmount;

    public float MoveSwayAmountX;
    public float MoveSwayAmountY;
    public float MoveSwayAmountZ;
    public float MaxMoveSwayAmount;
    public float MoveSmoothAmount;

    private Vector3 _initialPosition;
    private Vector3 _currentVelocitySway;

    private void Start()
    {
        _initialPosition = transform.localPosition;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        float mouseX = -Input.GetAxis("Mouse X") * MouseSwayAmount;
        float mouseY = -Input.GetAxis("Mouse Y") * MouseSwayAmount;

        mouseX = Mathf.Clamp(mouseX, -MaxMouseSwayAmount, MaxMouseSwayAmount);
        mouseY = Mathf.Clamp(mouseY, -MaxMouseSwayAmount, MaxMouseSwayAmount);

        Vector3 targetMouseSwayPosition = new Vector3(mouseX, mouseY, 0);

        Vector3 playerVelocity = PlayerLogic.PlayerData.Velocity;
        Vector3 localVelocity = PlayerLogic.transform.InverseTransformDirection(playerVelocity);

        float targetMoveSwayX = -localVelocity.x * MoveSwayAmountX;
        float targetMoveSwayY = -localVelocity.y * MoveSwayAmountY;
        float targetMoveSwayZ = -localVelocity.z * MoveSwayAmountZ;

        Vector3 targetMoveSwayPosition = new Vector3(targetMoveSwayX, targetMoveSwayY, targetMoveSwayZ);
        targetMoveSwayPosition = Vector3.ClampMagnitude(targetMoveSwayPosition, MaxMoveSwayAmount);

        _currentVelocitySway = Vector3.Lerp(_currentVelocitySway, targetMoveSwayPosition, Time.deltaTime * MoveSmoothAmount);

        Vector3 intermediatePosition = _initialPosition + targetMouseSwayPosition;

        transform.localPosition = Vector3.Lerp(transform.localPosition, intermediatePosition + _currentVelocitySway, Time.deltaTime * MouseSmoothAmount);
    }
}
