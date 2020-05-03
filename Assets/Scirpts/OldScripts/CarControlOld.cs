using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControlOld : MonoBehaviour
{
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform backLeftWheel;
    public Transform backRightWheel;

    public Transform frontLeftWheelModel;
    public Transform frontRightWheelModel;
    public Transform backLeftWheelModel;
    public Transform backRightWheelModel;

    public Transform cubeObject;

    public Transform car;
    public Rigidbody sphere;

    public float acceleration = 30f;
    public float steering = 80f;
    public float gravity = 10f;

    public float speedStep = 12f;
    public float rotateStep = 4f;

    float currentSpeed, speed;
    float currentRotate, rotate;

    Vector3 prevPosition;

    public float trackDistance = 5f;

    public float wheelRotationSpeed = 5f;

    void Update()
    {
        var velocity = transform.InverseTransformDirection(sphere.GetComponent<Rigidbody>().velocity);
        Control();
        RotateCar();
        GroundCheck();
        RotateWheels(velocity);
    }
    void GroundCheck()
    {
        RaycastHit groundHit;
        Physics.Raycast(transform.position, Vector3.down, out groundHit, 1f);
        //Debug.DrawLine(transform.position, hitNear.point, Color.red, 0.1f);

        car.up = Vector3.Lerp(car.up, groundHit.normal, 8.0f * Time.deltaTime);
        car.Rotate(0, transform.eulerAngles.y, 0);
    }
    void Control()
    {
        //speed = acceleration;
        speed = acceleration * Input.GetAxis("Vertical");

        /*if (Input.GetAxis("Horizontal") > 0)
            if (transform.rotation.y < 0.5f)
            {
                rotate = steering * Input.GetAxis("Horizontal");
            }
        if (Input.GetAxis("Horizontal") < 0)
            if (transform.rotation.y > -0.5f)
            {
                rotate = steering * Input.GetAxis("Horizontal");
            }*/
        rotate = steering * Input.GetAxis("Horizontal");

        currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * speedStep);
        currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * rotateStep);
    }
    void RotateCar()
    {
        transform.position = sphere.transform.position - new Vector3(0, 1.37f, 0f);
        transform.Rotate(0, currentRotate * Time.deltaTime, 0);
    }
    void RotateWheels(Vector3 velocity)
    {
        frontLeftWheelModel.Rotate(wheelRotationSpeed * Time.deltaTime * velocity.z, 0, 0, Space.Self);
        frontRightWheelModel.Rotate(wheelRotationSpeed * Time.deltaTime * velocity.z, 0, 0, Space.Self);
        backLeftWheelModel.Rotate(wheelRotationSpeed * Time.deltaTime * velocity.z, 0, 0, Space.Self);
        backRightWheelModel.Rotate(wheelRotationSpeed * Time.deltaTime * velocity.z, 0, 0, Space.Self);

        Vector3 newRotationLeft = car.forward;
        Vector3 newRotationRight = car.forward;
        Debug.Log(newRotationRight);
        if (Input.GetAxis("Horizontal") > -0.4 && Input.GetAxis("Horizontal") < 0.4)
        {
            if (Vector3.Distance(sphere.transform.position, cubeObject.position) > trackDistance)
            {
                newRotationLeft = cubeObject.position - frontLeftWheel.position;
                newRotationRight = cubeObject.position - frontRightWheel.position;
            }
            frontLeftWheel.forward = Vector3.Lerp(frontLeftWheel.forward, newRotationLeft, Time.deltaTime);
            frontRightWheel.forward = Vector3.Lerp(frontRightWheel.forward, newRotationRight, Time.deltaTime);
        }
        else
        {
            newRotationLeft = Quaternion.Euler(0, Input.GetAxis("Horizontal") * 45f, 0) * car.forward;
            newRotationRight = Quaternion.Euler(0, Input.GetAxis("Horizontal") * 45f, 0) * car.forward;
            frontLeftWheel.forward = Vector3.Lerp(frontLeftWheel.forward, newRotationLeft, Time.deltaTime * 12f);
            frontRightWheel.forward = Vector3.Lerp(frontRightWheel.forward, newRotationRight, Time.deltaTime * 12f);
        }
        frontLeftWheel.localEulerAngles = new Vector3(frontLeftWheel.localEulerAngles.x, ClampAngle(frontLeftWheel.localEulerAngles.y, -50f, 50f), frontLeftWheel.localEulerAngles.z);
        frontRightWheel.localEulerAngles = new Vector3(frontRightWheel.localEulerAngles.x, ClampAngle(frontRightWheel.localEulerAngles.y, -50f, 50f), frontRightWheel.localEulerAngles.z);
    }

    void FixedUpdate()
    {
        sphere.AddForce(car.transform.forward * currentSpeed, ForceMode.Acceleration);
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        Vector3 direction = (sphere.position - prevPosition)*10f;
        Debug.DrawRay(sphere.position, direction, Color.red, 0.05f);
        cubeObject.position = Vector3.Lerp(cubeObject.position, direction + sphere.position, 10f);
        prevPosition = sphere.position;
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
}
