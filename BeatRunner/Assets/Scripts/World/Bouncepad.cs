using System;
using BananaUtils.OnScreenDebugger.Scripts;
using P90brush;
using UnityEngine;

public class Bouncepad : MonoBehaviour
{
    [SerializeField] private Vector3 _launchForce;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerLogic playerLogic))
        {
            playerLogic.PlayerData.Velocity += _launchForce;
        }
    }
}
