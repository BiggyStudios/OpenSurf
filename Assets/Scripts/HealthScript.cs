using Mirror;

using P90brush;

using UnityEngine;

public class HealthScript : NetworkBehaviour
{
    public float MaxHealth;

    private float _health;

    private PlayerLogic _playerLogic;

    private void Start()
    {
        _playerLogic = GetComponent<PlayerLogic>();
        _health = MaxHealth;
    }

    [Server]
    public void TakeDamageOnServer(float amount)
    {
        if (_health <= 0) return;

        _health -= amount;

        if (_health <= 0)
        {
            Die();
        }
    }
    
    [ClientRpc]
    private void Die()
    {
        _playerLogic.PlayerData.Origin = Vector3.zero;
        _health = MaxHealth;
    }
}
