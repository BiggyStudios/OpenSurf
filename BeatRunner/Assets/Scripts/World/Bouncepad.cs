using System;
using BananaUtils.OnScreenDebugger.Scripts;
using SourceMovement.Movement;
using UnityEngine;

public class Bouncepad : MonoBehaviour
{
    [SerializeField] private Vector3 _launchForce;

    private void OnTriggerEnter(Collider other)
    {
        SurfCharacter playerScript = other.GetComponentInParent<SurfCharacter>();
        playerScript.moveData.velocity = Vector3.zero;
        playerScript.moveData.velocity += _launchForce;
    }
}
