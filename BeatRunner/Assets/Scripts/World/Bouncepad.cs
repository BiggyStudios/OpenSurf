using System;
using UnityEngine;

public class Bouncepad : MonoBehaviour
{
    [SerializeField] private Vector3 _launchForce;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerMovement playerMovement))
            playerMovement.gameObject.GetComponent<Rigidbody>().AddForce(_launchForce, ForceMode.Impulse);
    }
}
