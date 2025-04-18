using Mirror;

using P90brush;

using TMPro;

using UnityEngine;

public class HealthScript : NetworkBehaviour
{
    public TMP_Text HealthText;

    public float MaxHealth;

    private float _health;

    private PlayerLogic _playerLogic;

    private void Start()
    {
        _playerLogic = GetComponent<PlayerLogic>();
        _health = MaxHealth;

        HealthText.text = new string("Health:" + MaxHealth);
    }

    [Server]    
    public void UpdateHealthText()
    {
        HealthText.text = new string("Health:" + _health);
    }

    [Server]
    public void TakeDamageOnServer(float amount)
    {
        if (_health <= 0) return;

        _health -= amount;
        UpdateHealthText();

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
        
        UpdateHealthText();
    }
}
