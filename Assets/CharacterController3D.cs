using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController3D : MonoBehaviour
{    
    //This class is based on a previous character controller built for a spaceship. I am leaving in some of that content commented out for reference.
    //This class is intended for use with the StarterSampleScene

    Rigidbody m_Rigidbody;
    StarterControls controls;
    Vector2 slidemove;
    Vector2 pitchyaw;
    float m_Thrust;
    float forward;
    float backwards;
    float rollright;
    float rollleft;
    float drag;

    void Start()
    {
    m_Rigidbody = GetComponent<Rigidbody>();
        #if UNITY_STANDALONE_WIN
                m_Thrust = 40;
                Debug.Log("Standalone");
        #endif
        #if UNITY_EDITOR
                m_Thrust = 10;
                Debug.Log("Editor");
        #endif
    }

private void Awake()
{
    controls = new StarterControls();

    controls.gameplay.move.performed += ctx => slidemove = ctx.ReadValue<Vector2>();
    controls.gameplay.move.canceled += ctx => slidemove = Vector2.zero;
    controls.gameplay.look.performed += ctx => pitchyaw = ctx.ReadValue<Vector2>();
    controls.gameplay.look.canceled += ctx => pitchyaw = Vector2.zero;
    //controls.gameplay.fire.performed += ctx => forward = ctx.ReadValue<float>();
    //controls.gameplay.fire.canceled += ctx => forward = 0;
    //controls.gameplay.Reverse.performed += ctx => backwards = ctx.ReadValue<float>();
    //controls.gameplay.Reverse.canceled += ctx => backwards = 0;
    //controls.gameplay.RollRight.performed += ctx => rollright = ctx.ReadValue<float>();
    //controls.gameplay.RollRight.canceled += ctx => rollright = 0;
    //controls.gameplay.RollLeft.performed += ctx => rollleft = ctx.ReadValue<float>();
    //controls.gameplay.RollLeft.canceled += ctx => rollleft = 0;
    //controls.gameplay.Brake.performed += ctx => drag = ctx.ReadValue<float>();
    //controls.gameplay.Brake.canceled += ctx => drag = 0;
}

private void OnEnable()
{
    controls.gameplay.Enable();
}

void FixedUpdate()
{
    //Apply input values as forces against each thruster group
    m_Rigidbody.AddForce(transform.forward * slidemove.y * m_Thrust); //Slide vertical
    m_Rigidbody.AddForce(transform.right * slidemove.x * m_Thrust); //slide horizontal
    //m_Rigidbody.AddTorque(transform.right * pitchyaw.y * 0.1f * m_Thrust); //pitch
    m_Rigidbody.AddTorque(transform.up * pitchyaw.x * 0.1f * m_Thrust); //yaw
    //m_Rigidbody.AddForce(transform.forward * forward * 1f * m_Thrust); //forward
    //m_Rigidbody.AddForce(transform.forward * backwards * -1f * m_Thrust); //backwards
    //m_Rigidbody.AddTorque(transform.forward * rollright * -0.05f * m_Thrust); //rollright
    //m_Rigidbody.AddTorque(transform.forward * rollleft * 0.05f * m_Thrust); //rollleft
    //m_Rigidbody.drag = drag; //brake
    //m_Rigidbody.angularDrag = drag; //brake
}

}
