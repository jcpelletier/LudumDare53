using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController3D : MonoBehaviour
{
    //This class is based on a previous character controller built for a spaceship. I am leaving in some of that content commented out for reference.
    //This class is intended for use with the StarterSampleScene

    public float minSpeed = 0f;
    public float maxSpeed = 8f;

    [SerializeField]
    bool flaplock = false;
    [SerializeField]
    float flaplocktimeout;
    Rigidbody m_Rigidbody;
    StarterControls controls;
    Vector2 slidemove;
    Vector2 pitchyaw;
    Vector2 tilt;
    [SerializeField]
    float m_Thrust;
    float lift;
    float backwards;
    float rollright;
    float rollleft;
    float drag;
    float reset;

    public GameObject mail;
    public GameObject missiontarget;
    public GameObject bowerycanvas;
    public GameObject missioncanvas;
    public GameObject deathcanvas;

    //Sound References
    public AudioSource deathsound;
    public AudioSource flapsound;
    public AudioSource acceptmissionsound;
    public AudioSource completemissionsound;

    public float groundDistance = 1.6f;
    public LayerMask groundMask;

    [SerializeField]
    private bool isGrounded = false;

    public Vector3 newPosition = new Vector3(0, 0, 0); //for reset position
    public Vector3 newRotation = new Vector3(0, 0, 0); //for reset rotation

    //for Angle of Attack dynamic
    public float pitchUpForce = 1f;
    public float pitchDownForce = -1f;
    public float maxForceMagnitude = 10f;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

        newPosition = transform.position;
        newRotation = transform.rotation.eulerAngles;

        #if UNITY_STANDALONE_WIN
                m_Thrust = 0.1f;
                Debug.Log("Standalone");
        #endif
        #if UNITY_EDITOR
                m_Thrust = 0.1f;
                Debug.Log("Editor");
        #endif
    }

    private void Awake()
    {
        controls = new StarterControls();

        controls.gameplay.tilt.performed += ctx =>
        {
            tilt = ctx.ReadValue<Vector2>();

        };
        controls.gameplay.move.canceled += ctx => slidemove = Vector2.zero;
        controls.gameplay.move.performed += ctx =>
        {
            slidemove = ctx.ReadValue<Vector2>();

        };
        controls.gameplay.move.canceled += ctx => slidemove = Vector2.zero;
        controls.gameplay.look.performed += ctx =>
        {
            pitchyaw = ctx.ReadValue<Vector2>();
            
        };
        controls.gameplay.look.canceled += ctx => pitchyaw = Vector2.zero;
        controls.gameplay.lift.performed += ctx =>
        {
            lift = ctx.ReadValue<float>();
            if (bowerycanvas.activeInHierarchy)
            {
                bowerycanvas.SetActive(false);
                acceptmissionsound.Play();
            }
            if (missioncanvas.activeInHierarchy)
            {
                missioncanvas.SetActive(false);
                completemissionsound.Play();
            }
            flapsound.Play();
            isGrounded = false;
        };
        controls.gameplay.lift.canceled += ctx => lift = 0;
        controls.gameplay.reset.performed += ctx =>
        {
            SetPosition(newPosition, newRotation);
            isGrounded = false;
        };
        controls.gameplay.yawright.performed += ctx =>
        {
            pitchyaw.x = ctx.ReadValue<float>();
        };
        controls.gameplay.yawright.canceled += ctx => pitchyaw.x = 0;
        controls.gameplay.yawleft.performed += ctx =>
        {
            pitchyaw.x = -ctx.ReadValue<float>();
        };
        controls.gameplay.yawleft.canceled += ctx => pitchyaw.x = 0;

        //controls.gameplay.tilt.performed += ctx => tilt = ctx.ReadValue<float>();
        //controls.gameplay.tilt.canceled += ctx => tilt = 0;
        //controls.gameplay.RollLeft.performed += ctx => rollleft = ctx.ReadValue<float>();
        //controls.gameplay.RollLeft.canceled += ctx => rollleft = 0;
        //controls.gameplay.Brake.performed += ctx => drag = ctx.ReadValue<float>();
        //controls.gameplay.Brake.canceled += ctx => drag = 0;
    }

    private void OnEnable()
    {
        controls.gameplay.Enable();
    }

    private float GetAngleOfAttack()
    {
        Vector3 velocity = m_Rigidbody.velocity;
        Vector3 forward = transform.forward;
        return Vector3.Angle(velocity, forward);
    }

    private void SetPosition(Vector3 position, Vector3 rotation)
    {
        transform.position = position;
        transform.eulerAngles = rotation;
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    private void SetFlaplock()
    {
        flaplock = true;
        flaplocktimeout = 0.3f;
    }
 
    void FixedUpdate()
    {
        // Angle of Attack based movement
        float angle = GetAngleOfAttack();
        Vector3 forceDirection = transform.forward;
        float forceMagnitude = 0f;
        if (angle > 0f)
        {
            forceMagnitude = Mathf.Clamp(angle * pitchUpForce, 0f, maxForceMagnitude);
        }
        else if (angle < 0f)
        {
            forceMagnitude = Mathf.Clamp(angle * pitchDownForce, -maxForceMagnitude, 0f);
        }
        m_Rigidbody.AddForce(forceDirection * forceMagnitude * 0.00001f);

        if (flaplocktimeout <= 0)
        {
            flaplock = false;
            flaplocktimeout = 0.3f;
        }
        m_Rigidbody.AddTorque(transform.up * pitchyaw.x * 0.00002f); //yaw
        m_Rigidbody.AddTorque(transform.forward * -tilt.x * 0.00002f * m_Thrust); //roll
        m_Rigidbody.AddTorque(transform.right * tilt.y * 0.00002f * m_Thrust); //pitch
        if (!flaplock)
        { 
            //Apply input values as forces against each thruster group
            //m_Rigidbody.AddForce(transform.forward * slidemove.y * 0.05f * m_Thrust, ForceMode.Impulse); //Slide vertical
            m_Rigidbody.AddForce(transform.right * slidemove.x * 0.05f * m_Thrust, ForceMode.Impulse); //slide horizontal
            m_Rigidbody.AddForce(transform.up * lift * 0.01f * m_Thrust, ForceMode.Impulse); //Add up lift
            //m_Rigidbody.AddTorque(transform.right * pitchyaw.y * 0.1f * m_Thrust); //pitch
        

            //m_Rigidbody.AddForce(transform.forward * forward * 1f * m_Thrust); //forward
            //m_Rigidbody.AddForce(transform.forward * backwards * -1f * m_Thrust); //backwards
            //m_Rigidbody.AddTorque(transform.forward * rollright * -0.05f * m_Thrust); //rollright
            //m_Rigidbody.AddTorque(transform.forward * rollleft * 0.05f * m_Thrust); //rollleft
            //m_Rigidbody.drag = drag; //brake
            //m_Rigidbody.angularDrag = drag; //brake
            //if some sort of state is true then call SetFlaplock()
            
        }
        else { flaplocktimeout = flaplocktimeout - 0.1f; }
 
        if (slidemove.x != 0 || lift != 0)
        {
            SetFlaplock(); 
        }

        Vector3 velocity = m_Rigidbody.velocity;
        Vector3 forward = transform.forward;
        float angleOfAttack = Vector3.Angle(velocity, forward);
        float speed = velocity.magnitude;
        // Clamp speed between minSpeed and maxSpeed
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);

        // Set new velocity with clamped speed
        m_Rigidbody.velocity = velocity.normalized * speed;

        //isGrounded = Physics.Raycast(transform.position, Vector3.down, groundDistance);
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = true;
        float relativeVelocity = collision.relativeVelocity.magnitude;
        if (relativeVelocity > 3f)
        {
            SetPosition(newPosition, newRotation);
            deathsound.Play();
            deathcanvas.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        
        if (collision.gameObject.name == "TheBowery")
        {
            mail.SetActive(true);
            missiontarget.SetActive(true);
            bowerycanvas.SetActive(true);
        }
        if (collision.gameObject.name == "MissionTarget")
        {
            missiontarget.SetActive(false);
            missioncanvas.SetActive(true);
            mail.SetActive(false);
        }
    }
    

}
