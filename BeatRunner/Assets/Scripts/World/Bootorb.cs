using System;
using P90brush;
using UnityEngine;

public class Bootball : MonoBehaviour
{
   [SerializeField] private Vector3 _launchVector;
   
   private void OnTriggerEnter(Collider other)
   {
      if (other.TryGetComponent(out PlayerLogic playerLogic))
      {
         if (playerLogic.InputData.JumpPressed)
         {
            playerLogic.PlayerData.Velocity += _launchVector;
         }
      }
   }
}
