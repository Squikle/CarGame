using UnityEngine;
using UnityEngine.SceneManagement;

public class CarControlRayCast : MonoBehaviour
{
    [Header("Wheels")]
    public WheelsClass[] wheels;
    //public Transform[] wheels;
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    /*public Transform backLeftWheel;
    public Transform backRightWheel;*/
    public float wheelRadius=1f;

    [Header("Visualisation")]
    public ParticleSystem jumpEffect;
    //public Transform[] wheelModels;
    public float wheelRotationSpeed = 5f;
    public float trackDistance = 5f;
    [Header("BackLights")]
    public Light[] backLights;
    private float defaultBackLightsIntensity;


    [Header("Physic")]
    public float maxSpeed = 60f;
    public float acceleration = 30f;
    public float backAcceleration = 20f;
    public float steering = 80f;
    public float gravity = 10f;
    public float suspensionForce = 1f;
    public Transform centerOfMass;
    public Transform dynamicCenterOfMass;

    [Header("Raycast")]
    //public Transform[] rayPoints;
    public float suspensionStrength = 10f;
    public float raycastLenght=1f;


    Material groundMaterial;

    private Rigidbody rb;
    private float verticalAxis;
    private float HorizontalAxis;

    private Vector3 prevPosition;
    private Vector3 velocity;
    private Vector3 movingDirection;

    private bool grounded;
    private bool flipped;

    private float currentAcceleration;
    private float currentRotation;

    private float defaultDrag;


    private int layerMask;

    void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("Player");
        layerMask = ~layerMask;

        rb = GetComponent<Rigidbody>();

        defaultDrag = rb.drag;

        defaultBackLightsIntensity = backLights[0].intensity;
    }

    void Update()
    {
        velocity = transform.InverseTransformDirection(rb.velocity);

        ParticlesControl();
        Control();
        Suspension();
        RotateWheels(velocity);
    }

    void FixedUpdate()
    {
        //Debug.Log(velocity);

        RayCasting();
        CenterOfMassMovement();

        if (grounded && !flipped)
        {
            if (Mathf.Abs(currentAcceleration) > 0)
            {
                if (rb.velocity.magnitude > maxSpeed)
                    rb.velocity = rb.velocity.normalized * maxSpeed;
                else
                {
                    rb.AddForce(transform.forward * currentAcceleration, ForceMode.Acceleration);
                }
            }

            if (currentRotation > 0)
                rb.AddRelativeTorque(Vector3.up * currentRotation, ForceMode.Acceleration);
            else if (currentRotation < 0)
                rb.AddRelativeTorque(Vector3.up * currentRotation, ForceMode.Acceleration);
        }


        Vector3 direction = (transform.position - prevPosition) * 10f;
        Vector3 newMovingPos = Vector3.Lerp(movingDirection, direction + transform.position, 10f);
        newMovingPos.y += 1f;
        movingDirection = newMovingPos;
        prevPosition = transform.position;

        if (!grounded)
            rb.drag = 0;
        else
            if (verticalAxis < 0.5)
                rb.drag = 1.5f;
        else
            rb.drag = defaultDrag;



        foreach (Light light in backLights)
            light.intensity = defaultBackLightsIntensity;

        if (velocity.z > 3f && verticalAxis < 0.4f)
        {
            foreach (Light light in backLights)
                light.intensity = defaultBackLightsIntensity + 0.35f;
        }
    }

    void Control()
    {

        if(Input.GetKey(KeyCode.T))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        verticalAxis = Input.GetAxis("Vertical");
        HorizontalAxis = Input.GetAxis("Horizontal");

        if (verticalAxis > 0)
            currentAcceleration = acceleration * verticalAxis;
        else if (verticalAxis < 0)
            currentAcceleration = backAcceleration * verticalAxis;
        else currentAcceleration = 0;

        if (Mathf.Abs(velocity.z) > 0.05f)
            currentRotation = steering * HorizontalAxis * (1f - velocity.z / maxSpeed) * (velocity.z/maxSpeed); // 12.8 accel
        //currentRotation = steering * HorizontalAxis * (velocity.z / 32f); // 12.8 accel
    }

    void CenterOfMassMovement()
    {
        if (grounded && verticalAxis != 0)
        {
            Vector3 centerPosition = centerOfMass.position;
            float radiusOfMovement = 3f;

            Vector3 newPos = dynamicCenterOfMass.position + dynamicCenterOfMass.forward * -verticalAxis * Time.deltaTime * 18f;
            //newPos = newPos + dynamicCenterOfMass.right * HorizontalAxis * Time.deltaTime * 10f;

            float distance = Vector3.Distance(newPos, centerPosition);

            if (distance > radiusOfMovement)
            {
                Vector3 fromOriginToObject = newPos - centerPosition;
                fromOriginToObject *= radiusOfMovement / distance;
                newPos = centerPosition + fromOriginToObject;
            }

            dynamicCenterOfMass.position = newPos;
        }
        else dynamicCenterOfMass.position = Vector3.MoveTowards(dynamicCenterOfMass.position, centerOfMass.position, Time.deltaTime * 18f);
    }

    void RayCasting()
    {
        RaycastHit hit;
        foreach (WheelsClass wheel in wheels)
        {
            if (Physics.Raycast(wheel.raySource.position, -transform.up, out hit, raycastLenght, layerMask))
            {
                groundMaterial = hit.collider.GetComponent<MeshRenderer>().material;
                rb.AddForceAtPosition(Vector3.up * suspensionStrength * Mathf.Pow((1.0f - hit.distance / raycastLenght), 0.8f), wheel.raySource.position, ForceMode.Acceleration); // степень - зависимость силы от дистанции до земли
                if (!grounded && !flipped && Mathf.Abs(velocity.y) > 15f)
                {
                    jumpEffect.GetComponent<ParticleSystemRenderer>().material = groundMaterial;
                    GameObject groundingEffect = Instantiate(jumpEffect.gameObject, transform.position, Quaternion.Euler(90, 0, 0));
                    Destroy(groundingEffect, 1f);
                }
                grounded = true;
                flipped = false;
            }
            else
            {
               //rb.AddForceAtPosition(Vector3.down * gravity, centerOfMass.position, ForceMode.Acceleration);
               rb.AddForceAtPosition(gravity*Vector3.down, centerOfMass.position, ForceMode.Acceleration);
               grounded = false;
            }
        }

        if (Physics.Raycast(transform.position, transform.up, out hit, 2.5f, layerMask))
        {
            grounded = true;
            flipped = true;
        }
    }
    void Suspension()
    {
        if (Mathf.Abs(velocity.z) / maxSpeed < 0.4f && verticalAxis != 0)
        {
            rb.AddForceAtPosition(-dynamicCenterOfMass.up * suspensionForce, dynamicCenterOfMass.position, ForceMode.Acceleration);
        }

        RaycastHit hit;

        foreach (WheelsClass wheel in wheels)
        {
            if (Physics.Raycast(wheel.raySource.position, -wheel.raySource.up, out hit, raycastLenght, layerMask))
            {
                Vector3 groundedPosition = hit.point + wheel.wheel.up * wheelRadius;
                if (wheel.wheelModel.position.y + 1f > groundedPosition.y)
                    wheel.wheelModel.position = Vector3.Lerp(wheel.wheelModel.position, groundedPosition, Time.deltaTime * 10f);
            }
            else 
                wheel.wheelModel.position = Vector3.Lerp(wheel.wheelModel.position, wheel.wheel.position, Time.deltaTime * 8f);
        }
    }

    void RotateWheels(Vector3 velocity)
    {
        foreach(WheelsClass wheel in wheels)
        {
            if (!flipped && Mathf.Abs(velocity.z)>0.1f)
                wheel.wheelModel.Rotate(wheelRotationSpeed * Time.deltaTime * velocity.z, 0, 0, Space.Self);
            else wheel.wheelModel.Rotate(wheelRotationSpeed * Time.deltaTime * verticalAxis * acceleration * 0.2f, 0, 0, Space.Self);
        }

        Vector3 newRotationLeft = transform.forward;
        Vector3 newRotationRight = transform.forward;
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.4f)
        {
            if (Vector3.Distance(transform.position, movingDirection) > trackDistance && velocity.z > -1f)
            {
                newRotationLeft = movingDirection - frontLeftWheel.position;
                newRotationRight = movingDirection - frontRightWheel.position;
            }
            frontLeftWheel.forward = Vector3.Lerp(frontLeftWheel.forward, newRotationLeft, Time.deltaTime);
            frontRightWheel.forward = Vector3.Lerp(frontRightWheel.forward, newRotationRight, Time.deltaTime);
        }
        else
        {
            newRotationLeft = Quaternion.Euler(frontLeftWheel.eulerAngles.x, Input.GetAxis("Horizontal") * 45f, frontLeftWheel.eulerAngles.z) * transform.forward;
            newRotationRight = Quaternion.Euler(frontLeftWheel.eulerAngles.x, Input.GetAxis("Horizontal") * 45f, frontLeftWheel.eulerAngles.z) * transform.forward;
            frontLeftWheel.forward = Vector3.Lerp(frontLeftWheel.forward, newRotationLeft, Time.deltaTime * 12f);
            frontRightWheel.forward = Vector3.Lerp(frontRightWheel.forward, newRotationRight, Time.deltaTime * 12f);
        }
        frontLeftWheel.localEulerAngles = new Vector3(0, ClampAngle(frontLeftWheel.localEulerAngles.y, -50f, 50f), 0);
        frontRightWheel.localEulerAngles = new Vector3(0, ClampAngle(frontRightWheel.localEulerAngles.y, -50f, 50f), 0);
    }

    void ParticlesControl()
    {

        if (!grounded)
            foreach (WheelsClass wheel in wheels)
                wheel.particleSource.Stop();
        else
            foreach (WheelsClass wheel in wheels)
            {
                wheel.particleSource.GetComponent<ParticleSystemRenderer>().material = groundMaterial;
                wheel.particleSource.Play();
            }
    }



    float ClampAngle(float angle, float min, float max)
    {
        if (angle < 90 || angle > 270) {       // if angle in the critic region...
            if (angle > 180) angle -= 360;  // convert all angles to -180..+180
            if (max > 180) max -= 360;
            if (min > 180) min -= 360;
        }
        angle = Mathf.Clamp(angle, min, max);
        if (angle < 0) angle += 360;  // if angle negative, convert to 0..360
        return angle;
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        foreach (var wheel in wheels)
            Gizmos.DrawSphere(wheel.wheel.position,0.2f);
        Gizmos.color = Color.green;
        foreach (var wheel in wheels)
            Gizmos.DrawSphere(wheel.wheel.position, 0.15f);

        Gizmos.color = Color.red;
        RaycastHit hit;
        foreach (var wheel in wheels)
        {
            Gizmos.DrawSphere(wheel.raySource.position, 0.1f);
            if (Physics.Raycast(wheel.raySource.position, -transform.up, out hit, raycastLenght))
            {
                Gizmos.DrawLine(wheel.raySource.position, hit.point);
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
            else
            {
                Gizmos.DrawLine(wheel.raySource.position, wheel.raySource.position - transform.up * raycastLenght);
            }
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, movingDirection);
        Gizmos.DrawSphere(movingDirection, 0.2f);
    }
}
