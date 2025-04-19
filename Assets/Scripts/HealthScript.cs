using Mirror;

using P90brush;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class HealthScript : NetworkBehaviour
{
    public Slider Healthbar;
    public TMP_Text HealthText;

    public float MaxHealth;

    private float _health;

    private PlayerLogic _playerLogic;

    private void Start()
    {
        _playerLogic = GetComponent<PlayerLogic>();
        _health = MaxHealth;

        SetHealthBar(MaxHealth);
    }

    [Server]    
    public void UpdateHealthBar()
    {
        SetHealthBar(_health);
    }

    [Server]
    public void TakeDamageOnServer(float amount)
    {
        if (_health <= 0) return;

        _health -= amount;
        UpdateHealthBar();

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
        
        UpdateHealthBar();
    }

    private void SetHealthBar(float value)
    {
        Healthbar.value = value;
        HealthText.text = new string(value + "/100+");
    }
}
