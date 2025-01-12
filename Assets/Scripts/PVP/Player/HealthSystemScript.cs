using UnityEngine;

public class HealthSystemScript : MonoBehaviour
{
    [SerializeField] private float _maxHealth;
    private float _health;

    private void Start()
    {
        _health = _maxHealth;
    }

    public void Damage(float amount)
    {
        _health -= amount;
    }
}
