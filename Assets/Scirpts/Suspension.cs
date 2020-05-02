using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Suspension : MonoBehaviour
{
    public Transform[] rayCastPoints;
    public Transform centerOfMovement;

    public float suspensionLenght = 1f;
    public float stiffness = 2000f;
    public float damping = 50f;

    public float maxSpeed = 60f;
    public float acceleration = 7000f;
    public float backAcceleration = 5000f;
    public float steering = 10000f;
    public float gravity = 5f;

    private LayerMask layerMask;
    private Rigidbody rb;

    private Vector3 velocity;
    private float verticalAxis;
    private float HorizontalAxis;
    private float currentAcceleration;
    private float currentRotation;

    private bool grounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        layerMask = 1 << LayerMask.NameToLayer("Player");
        layerMask = ~layerMask;
    }

    void Update()
    {
        velocity = transform.InverseTransformDirection(rb.velocity);
        Control();
    }


    void suspensionForce()
    {
        RaycastHit hit;

        grounded = false;

        foreach (Transform raySource in rayCastPoints)
        {
            if (Physics.Raycast(raySource.transform.position, -transform.up, out hit, suspensionLenght, layerMask))
            {
                grounded = true;

                float distance = suspensionLenght - hit.distance;
                float force = stiffness * distance + (-damping * rb.GetPointVelocity(raySource.transform.position).y);

                rb.AddForceAtPosition(transform.up * force, raySource.transform.position, ForceMode.Force);
            }
            else
                rb.AddForceAtPosition(Vector3.down*gravity, raySource.transform.position, ForceMode.Acceleration);
        }
    }

    void controlForce()
    {
        if (grounded)
        {
            if (Mathf.Abs(currentAcceleration) > 0)
            {
                if (rb.velocity.magnitude > maxSpeed)
                    rb.velocity = rb.velocity.normalized * maxSpeed;
                else
                {
                    Vector3 groundNormal;
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, -transform.up, out hit, 10f, layerMask))
                    {
                        groundNormal = hit.normal;
                        Vector3 force = Vector3.ProjectOnPlane(transform.forward * currentAcceleration, groundNormal);
                        rb.AddForceAtPosition(force, centerOfMovement.transform.position, ForceMode.Force);
                    }
                }
            }

            if (currentRotation > 0)
                rb.AddTorque(Vector3.up * currentRotation, ForceMode.Force);
            else if (currentRotation < 0)
                rb.AddTorque(Vector3.up * currentRotation, ForceMode.Force);
        }
    }
    void FixedUpdate()
    {
        suspensionForce();
        controlForce();
    }

    void Control()
    {
        verticalAxis = Input.GetAxis("Vertical");
        HorizontalAxis = Input.GetAxis("Horizontal");

        if (verticalAxis > 0)
            currentAcceleration = acceleration * verticalAxis;
        else if (verticalAxis < 0)
            currentAcceleration = backAcceleration * verticalAxis;
        else currentAcceleration = 0;

        if (Mathf.Abs(velocity.z) > 0.05f)
            currentRotation = steering * HorizontalAxis * (1f - velocity.z / maxSpeed) * (velocity.z / maxSpeed);
    }

    void OnDrawGizmos()
    {
        RaycastHit hit;
        foreach (Transform raySource in rayCastPoints)
        {
            Gizmos.color = Color.red;
            if (Physics.Raycast(raySource.transform.position, -transform.up, out hit, suspensionLenght, layerMask))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(raySource.transform.position, hit.point);
                Gizmos.DrawCube(hit.point, Vector3.one * 0.1f);
            }
            else 
            {
                Gizmos.DrawLine(raySource.transform.position, raySource.transform.position - transform.up * suspensionLenght);
                Gizmos.DrawCube(raySource.transform.position - transform.up * suspensionLenght, Vector3.one*0.1f);
            }
            Gizmos.DrawCube(raySource.transform.position, Vector3.one * 0.1f);


            Gizmos.color = Color.green;
            if (Physics.Raycast(transform.position, -transform.up, out hit, 10f, layerMask))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(hit.point, Vector3.one * 0.1f);
                Gizmos.DrawLine(transform.position, hit.point);

                //normal
                Gizmos.DrawLine(hit.point, hit.point + hit.normal * 2f);
            }
            Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);
        }
    }
}
