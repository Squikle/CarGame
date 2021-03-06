﻿using System;
using UnityEngine;

public class CarPhysic : MonoBehaviour
{
    public CarStats carStats;
    private ParticlesController particlesController;

    [Header("Wheels")]
    public WheelsClass[] wheels;
    public Transform frontLeftWheel;
    public Transform frontRightWheel;

    [Header("Visualisation")]
    private float wheelRotationSpeed = 60f;
    private float trackDistance = 5f;

    [Header("BackLights")]
    public Light[] backLights;
    private float defaultBackLightsIntensity;

    [Header("Physic")]
    private float gravity = 8f;
    public Transform centerOfMovement;

    private Vector3 prevPosition;
    private Vector3 movingDirection;

    [NonSerialized] public Rigidbody rb;
    [NonSerialized] public Vector3 velocity;

    [NonSerialized] public bool grounded;
    [NonSerialized] public bool flipped;
    [NonSerialized] public bool landing;

    [NonSerialized] public int layerMask;

    [NonSerialized] public float verticalAxis;
    [NonSerialized] public float HorizontalAxis;

    [NonSerialized] public GameObject surface;

    private float defaultDrag;

    void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("Player");
        layerMask = ~layerMask;

        rb = GetComponent<Rigidbody>();
        particlesController = GetComponent<ParticlesController>();

        defaultDrag = rb.drag;

        defaultBackLightsIntensity = backLights[0].intensity;
    }

    void Update()
    {
        velocity = transform.InverseTransformDirection(rb.velocity);

        movingDirectionCalculate();
        rotateWheels(velocity);
    }

    void FixedUpdate()
    {
        //Debug.Log(velocity);

        rayCasting();
        dragControl();

        foreach (Light light in backLights)
            light.intensity = defaultBackLightsIntensity;

        if (velocity.z > 3f && verticalAxis < 0.4f)
        {
            foreach (Light light in backLights)
                light.intensity = defaultBackLightsIntensity + 0.35f;
        }
    }

    void dragControl()
    {
        if (!grounded)
            rb.drag = 0;
        else
    if (Mathf.Abs(verticalAxis) < 0.1f)
            rb.drag = 2.5f;
        else
            rb.drag = defaultDrag;
    }

    void movingDirectionCalculate()
    {
        Vector3 direction = (transform.position - prevPosition) * 10f;
        Vector3 newMovingPos = Vector3.Lerp(movingDirection, direction + transform.position, 10f);
        newMovingPos.y += 1f;
        movingDirection = newMovingPos;
        prevPosition = transform.position;
    }

    void rayCasting()
    {
        RaycastHit hit;

        grounded = false;

        foreach (WheelsClass wheel in wheels)
        {
            if (Physics.Raycast(wheel.raySource.transform.position, -transform.up, out hit, carStats.suspensionLenght, layerMask))
            {
                grounded = true;
                flipped = false;

                float distance = carStats.suspensionLenght - hit.distance;
                float force = carStats.suspensionStrength * distance + (-carStats.damping * rb.GetPointVelocity(wheel.raySource.transform.position).y);

                rb.AddForceAtPosition(transform.up * force, wheel.raySource.transform.position, ForceMode.Force);
            }
            else
            {
                wheel.surface = null;
                rb.AddForceAtPosition(Vector3.down * gravity, wheel.raySource.transform.position, ForceMode.Acceleration);
            }
        }

        if (!grounded) landing = true;
        if (grounded && !flipped && velocity.y < -15f && landing)
        {
            particlesController.land();
            landing = !landing;
        }

        if (Physics.Raycast(transform.position, transform.up, out hit, 2.5f, layerMask))
        {
            grounded = true;
            flipped = true;
        }

        foreach (WheelsClass wheel in wheels)
        {
            if (Physics.Raycast(wheel.raySource.position, -wheel.raySource.up, out hit, carStats.suspensionLenght, layerMask))
            {
                Vector3 groundedPosition = hit.point + wheel.wheel.up * carStats.wheelRadius;

                if (groundedPosition.y > wheel.wheel.position.y)
                    groundedPosition = wheel.wheel.position;

                wheel.wheelModel.position = Vector3.Lerp(wheel.wheelModel.position, groundedPosition, Time.deltaTime * 10f);
            }
            else
                wheel.wheelModel.position = Vector3.Lerp(wheel.wheelModel.position, wheel.wheel.position, Time.deltaTime * 8f);
        }
    }

    void rotateWheels(Vector3 velocity)
    {
        foreach (WheelsClass wheel in wheels)
        {
            if (!flipped && Mathf.Abs(velocity.z) > 0.01f)
                wheel.wheelModel.Rotate(wheelRotationSpeed * Time.deltaTime * velocity.z, 0, 0, Space.Self);
            else wheel.wheelModel.Rotate(wheelRotationSpeed * Time.deltaTime * verticalAxis * carStats.acceleration * 0.2f, 0, 0, Space.Self);
        }

        Vector3 newRotationLeft = transform.forward;
        Vector3 newRotationRight = transform.forward;
        if (Mathf.Abs(HorizontalAxis) < 0.4f)
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
            newRotationLeft = Quaternion.Euler(frontLeftWheel.eulerAngles.x, HorizontalAxis * 45f, frontLeftWheel.eulerAngles.z) * transform.forward;
            newRotationRight = Quaternion.Euler(frontLeftWheel.eulerAngles.x, HorizontalAxis * 45f, frontLeftWheel.eulerAngles.z) * transform.forward;
            frontLeftWheel.forward = Vector3.Lerp(frontLeftWheel.forward, newRotationLeft, Time.deltaTime * 12f);
            frontRightWheel.forward = Vector3.Lerp(frontRightWheel.forward, newRotationRight, Time.deltaTime * 12f);
        }
        frontLeftWheel.localEulerAngles = new Vector3(0, ClampAngle(frontLeftWheel.localEulerAngles.y, -50f, 50f), 0);
        frontRightWheel.localEulerAngles = new Vector3(0, ClampAngle(frontRightWheel.localEulerAngles.y, -50f, 50f), 0);
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < 90 || angle > 270)
        {       // if angle in the critic region...
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
        RaycastHit hit;
        foreach (WheelsClass wheel in wheels)
        {
            Gizmos.color = Color.red;
            if (Physics.Raycast(wheel.raySource.transform.position, -transform.up, out hit, carStats.suspensionLenght, layerMask))
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(wheel.raySource.transform.position, hit.point);
                Gizmos.DrawCube(hit.point, Vector3.one * 0.1f);
            }
            else
            {
                Gizmos.DrawLine(wheel.raySource.transform.position, wheel.raySource.transform.position - transform.up * carStats.suspensionLenght);
                Gizmos.DrawCube(wheel.raySource.transform.position - transform.up * carStats.suspensionLenght, Vector3.one * 0.1f);
            }
            Gizmos.DrawCube(wheel.raySource.transform.position, Vector3.one * 0.1f);


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
