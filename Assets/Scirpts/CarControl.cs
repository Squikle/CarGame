using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(CarPhysic))]
public class CarControl : MonoBehaviour
{
    private CarPhysic carPhysic;


    private float currentAcceleration;
    private float currentRotation;



    void Start() => carPhysic = GetComponent<CarPhysic>();
    void Update()
    {
        TouchButtonsInput();
        Control();
    }
    void FixedUpdate() => ControlForce();

    void TouchButtonsInput()
    {
        carPhysic.verticalAxis = CrossPlatformInputManager.GetAxis("Vertical");
        carPhysic.HorizontalAxis = CrossPlatformInputManager.GetAxis("Horizontal");
    }

    void TouchInputCheck()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);


            if (touch.phase != TouchPhase.Ended)
            {
                carPhysic.verticalAxis = 1;
                carPhysic.HorizontalAxis = (touch.position.x - Screen.width/2)*2 / Screen.width;
                Debug.Log("Phase: " + touch.phase + " Place: " + touch.position + " Horizontal: " + carPhysic.HorizontalAxis);
            }
            else
            {
                carPhysic.verticalAxis = 0;
                carPhysic.HorizontalAxis = 0;
            }
        }
    }
    void InputCheck()
    {
        if (Input.GetKey(KeyCode.T))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        carPhysic.verticalAxis = Input.GetAxis("Vertical");
        carPhysic.HorizontalAxis = Input.GetAxis("Horizontal");
    }
    void Control()
    {
        if (carPhysic.verticalAxis > 0)
            currentAcceleration = carPhysic.carStats.acceleration * carPhysic.verticalAxis;
        else if (carPhysic.verticalAxis < 0)
            currentAcceleration = carPhysic.carStats.backAcceleration * carPhysic.verticalAxis;
        else currentAcceleration = 0;

        if (Mathf.Abs(carPhysic.velocity.z) > 0.01f)
            currentRotation = carPhysic.carStats.steering * carPhysic.HorizontalAxis * (1f - carPhysic.velocity.z / carPhysic.carStats.maxSpeed) * (carPhysic.velocity.z / carPhysic.carStats.maxSpeed);
        //currentRotation = carPhysic.carStats.steering * carPhysic.HorizontalAxis;
    }

    void ControlForce()
    {
        if (carPhysic.grounded)
        {
            if (Mathf.Abs(currentAcceleration) > 0)
            {
                if (carPhysic.rb.velocity.magnitude > carPhysic.carStats.maxSpeed)
                    carPhysic.rb.velocity = carPhysic.rb.velocity.normalized * carPhysic.carStats.maxSpeed;
                else
                {
                    Vector3 groundNormal;
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, -transform.up, out hit, 10f, carPhysic.layerMask))
                    {
                        groundNormal = hit.normal;
                        Vector3 force = Vector3.ProjectOnPlane(transform.forward * currentAcceleration, groundNormal);
                        carPhysic.rb.AddForceAtPosition(force, carPhysic.centerOfMovement.transform.position, ForceMode.Force);
                    }
                }
            }

            if (currentRotation > 0)
                carPhysic.rb.AddTorque(Vector3.up * currentRotation, ForceMode.Force);
            else if (currentRotation < 0)
                carPhysic.rb.AddTorque(Vector3.up * currentRotation, ForceMode.Force);
        }
    }
}
