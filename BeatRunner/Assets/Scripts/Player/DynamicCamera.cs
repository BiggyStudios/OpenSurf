using System;
using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
   private Animator _animator;

   private void Start() => _animator = GetComponent<Animator>();

   public void OnJump() => _animator.SetTrigger("Jump");

   public void OnLand() => _animator.SetTrigger("Land");
}
