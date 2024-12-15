
using UnityEngine;
using UnityEngine.UI;

namespace P90brush
{
    [System.Serializable]
    public class MovementConfig
    {
        public float Gravity = 24f;
        public bool AutoBhop = true;
        public float AirCap = 0.575f;
        public float AirAccel = 800f;
        public float Accel = 10f;
        public float Friction = 4f;
        public float AirFriction = 0.25f;
        public float StopSpeed = 1.905f;
        public float JumpPower = 8f;
        public float JumpHeight = 1.5f;
        public float MaxSpeed = 8f;
        public float MaxVelocity = 200f;
        public float StepSize = 0.5f;

        public float XSens = 20f;
        public float YSens = 20f;

        [Header("Input Settings")]
        public KeyCode MoveLeft = KeyCode.A;
        public KeyCode MoveRight = KeyCode.D;
        public KeyCode MoveForward = KeyCode.W;
        public KeyCode MoveBack = KeyCode.S;
        public KeyCode JumpButton = KeyCode.Space;
        public KeyCode ResetButton = KeyCode.R;
        public string HookButton = "Fire1";
        public KeyCode HookRetractButton = KeyCode.LeftShift;
        public KeyCode SlowMotion = KeyCode.LeftControl;

        public bool ClampAirSpeed;
        public float Bounce;
    }
}

