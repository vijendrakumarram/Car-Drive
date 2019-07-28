using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using UnityStandardAssets.CrossPlatformInput;

public class ThirdPersonInput : MonoBehaviour
{
    public FixedJoystick LeftJoystick;
    float h;
    float v;
    //public FixedButton Button;
    //public FixedTouchField TouchField;
    CarUserControl Control;

    //protected float CameraAngle;
    //protected float CameraAngleSpeed = 0.2f;

    // Use this for initialization
    private CarController m_Car; // the car controller we want to use
    private void Awake()
    {
        // get the car controller
        m_Car = GetComponent<CarController>();
    }

    void Update()
    {
        //h = LeftJoystick.inputVector.x;
        //v = LeftJoystick.inputVector.y;
        h = SimpleInput.GetAxis("Horizontal");
        v = SimpleInput.GetAxis("Vertical");
        float handbrake = CrossPlatformInputManager.GetAxis("Jump");
        m_Car.Move(h, v, v, handbrake);

    }
}
