using System;
using UnityEngine;

public class Bouncepad : MonoBehaviour
{
    [SerializeField] private Vector3 _launchForce;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerMovement playerMovement))
        {
            Rigidbody playerRb = playerMovement.gameObject.GetComponent<Rigidbody>();
            playerRb.velocity = Vector3.zero;
            playerRb.AddForce(_launchForce, ForceMode.Impulse);
        }
    }
}
