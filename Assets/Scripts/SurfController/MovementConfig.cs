
using UnityEngine;
using UnityEngine.UI;

namespace P90brush
{
    [System.Serializable]
    public class MovementConfig
    {
        public float Gravity        = 15.24f;
        public bool AutoBhop        = true;
        public float AirCap         = 0.575f;
        public float AirAccel       = 15000;
        public float Accel          = 7.62f;
        public float Friction       = 4f;
        public float AirFriction    = 0.25f;
        public float StopSpeed      = 1.905f;
        public float JumpPower      = 5.112f;
        public float JumpHeight     = 1.5f;
        public float MaxSpeed       = 6f;
        public float MaxVelocity    = 100f;
        public float StepSize       = 0.5f;

        public float XSens          = 300f;
        public float YSens          = 300f;

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

