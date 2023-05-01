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

    //upturn fallen over bird
    //[SerializeField] private float angleThreshold = 45f; // The angle threshold to consider the object on its side
    [SerializeField] private float rotationSpeed = 1f; // The speed at which the object rotates to the upright position

    public GameObject mail;
    public GameObject boweryTarget;
    public GameObject missiontarget1;
    public GameObject missiontarget2;
    public GameObject missiontarget3;
    public GameObject bowerycanvas;
    public GameObject missioncanvas1;
    public GameObject missioncanvas2;
    public GameObject missioncanvas3;
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

    //Mission progress
    int missionProgress;

    void Start()
    {
        missionProgress = 1;
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
        //controls.gameplay.move.canceled += ctx => slidemove = Vector2.zero;
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
                UnfreezePosition();
            }
            if (missioncanvas1.activeInHierarchy)
            {
                missioncanvas1.SetActive(false);
                completemissionsound.Play();
                UnfreezePosition();
            }
            if (missioncanvas2.activeInHierarchy)
            {
                missioncanvas2.SetActive(false);
                completemissionsound.Play();
                UnfreezePosition();
            }
            if (missioncanvas3.activeInHierarchy)
            {
                missioncanvas3.SetActive(false);
                completemissionsound.Play();
                UnfreezePosition();
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

    public bool IsSideways()
    {
        // Get the rotation angles of the target GameObject
        Vector3 rotationAngles = gameObject.transform.eulerAngles;
        //Debug.Log($"Checking X:{rotationAngles.x} Z:{rotationAngles.z}");
        // Check if the GameObject has been rotated 90 degrees or more around 2 axis
        if ((Mathf.Abs(rotationAngles.x) >= 88 && Mathf.Abs(rotationAngles.x) <= 272) || (Mathf.Abs(rotationAngles.z) >= 88 && Mathf.Abs(rotationAngles.z) <= 272))
        {
            //Debug.Log($"The GameObject is rotated 88 degrees or more. X:{rotationAngles.x} Z:{rotationAngles.z}");
            return true;
        }
        else
        {
            //Debug.Log("The GameObject is rotated less than 88 degrees.");
            return false;
        }
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
        m_Rigidbody.AddForce(forceDirection * forceMagnitude * 0.00002f);

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
            m_Rigidbody.AddForce(transform.right * slidemove.x * 0.01f * m_Thrust, ForceMode.Impulse); //slide horizontal
            m_Rigidbody.AddForce(transform.up * lift * 0.01f * m_Thrust, ForceMode.Impulse); //Add up lift
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
        if (isGrounded && IsSideways())
        {
            SmoothUpturn();
            if (speed <= 0.03f)
            {
                Debug.Log($"Sideways Stuck!!!, speed is {speed}, limit is 0.03");
                RaiseObjectBy(1.0f);
                transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);                
                isGrounded = false;
            }
        }
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
            if (missionProgress == 1)
            {
                missiontarget1.SetActive(true);
            }
            else if (missionProgress == 2) 
            {
                missiontarget2.SetActive(true);
            }
            else if (missionProgress == 3)
            {
                missiontarget3.SetActive(true);
            }
            bowerycanvas.SetActive(true);
            FreezePosition();
            boweryTarget.SetActive(false);
            missionProgress++;
        }
        if (collision.gameObject.name == "MissionTarget1")
        {
            missiontarget1.SetActive(false);
            missioncanvas1.SetActive(true);
            mail.SetActive(false);
            FreezePosition();
            boweryTarget.SetActive(true);
        }
        if (collision.gameObject.name == "MissionTarget2")
        {
            missiontarget2.SetActive(false);
            missioncanvas2.SetActive(true);
            mail.SetActive(false);
            FreezePosition();
            boweryTarget.SetActive(true);
        }
        if (collision.gameObject.name == "MissionTarget3")
        {
            missiontarget3.SetActive(false);
            missioncanvas3.SetActive(true);
            mail.SetActive(false);
            FreezePosition();
            boweryTarget.SetActive(true);
        }
    }
    void SmoothUpturn()
    {
        // Calculate the target rotation angles for the upright position
        Vector3 currentEulerAngles = transform.eulerAngles;
        float xRotation = (currentEulerAngles.x > 180) ? currentEulerAngles.x - 360 : currentEulerAngles.x;
        float zRotation = (currentEulerAngles.z > 180) ? currentEulerAngles.z - 360 : currentEulerAngles.z;

        // Determine if x or z rotation is larger in magnitude
        bool useXRotation = Mathf.Abs(xRotation) > Mathf.Abs(zRotation);

        // Set the rotation to the closest multiple of 90 degrees
        float targetXRotation = useXRotation ? Mathf.Round(xRotation / 90) * 90 : 0;
        float targetZRotation = useXRotation ? 0 : Mathf.Round(zRotation / 90) * 90;

        // Calculate the target rotation as a Quaternion
        Quaternion targetRotation = Quaternion.Euler(targetXRotation, currentEulerAngles.y, targetZRotation);

        // Smoothly rotate the object to the upright position using Slerp
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    bool IsUpsideDownStationary()
    {
        // Get the current rotation angles of the object
        Vector3 rotationAngles = transform.eulerAngles;

        // Normalize the angles to the range of [0, 360)
        rotationAngles.y = rotationAngles.y % 360;

        // Check if the object is upside down based on the angle threshold
        bool isUpsideDown = Mathf.Abs(rotationAngles.y) > 180 - 180 && Mathf.Abs(rotationAngles.y) < 180 + 180;

        // Check if the object's velocity is below the stationary threshold
        bool isStationary = m_Rigidbody.velocity.magnitude < 0.1f;

        // Return true if the object is upside down and stationary
        return isUpsideDown && isStationary;
    }

    public void RaiseObjectBy(float raiseAmount)
    {
        // Get the current position of the object
        Vector3 currentPosition = transform.position;

        // Raise the object by the specified raiseAmount on the y-axis
        currentPosition.y += raiseAmount;

        // Apply the updated position to the object
        transform.position = currentPosition;
    }

    public void FreezePosition()
    {
        // Store the current velocity before freezing
        //storedVelocity = rb.velocity;

        // Freeze the position of the Rigidbody along the x, y, and z axes
        m_Rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    public void UnfreezePosition()
    {
        // Unfreeze the position of the Rigidbody along the x, y, and z axes
        m_Rigidbody.constraints = RigidbodyConstraints.None;

        // Reapply the stored velocity to the Rigidbody
        //rb.velocity = storedVelocity;
    }


}
