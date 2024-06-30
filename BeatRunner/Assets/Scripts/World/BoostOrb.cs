using System;
using BananaUtils.OnScreenDebugger.Scripts;
using P90brush;
using UnityEngine;

public class BoostOrb : MonoBehaviour
{
   [SerializeField] private Vector3 _launchVector;
   
   private void OnTriggerEnter(Collider other)
   {
      if (other.TryGetComponent(out PlayerLogic playerLogic))
      {
         playerLogic.PlayerData.Velocity.y = 0;
         playerLogic.PlayerData.Velocity += _launchVector;
      }
   }
}
