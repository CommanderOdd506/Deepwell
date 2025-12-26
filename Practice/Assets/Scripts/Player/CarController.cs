using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviourPun
{
    [Header("References")]
    public Rigidbody rigid;
    public WheelCollider frontLeft, frontRight, rearLeft, rearRight;

    [Header("Car Settings")]
    public float driveSpeed = 1500f;
    public float steerSpeed = 30f;
    public float brakeForce = 2000f;
    public float maxSpeed = 50f;

    private float horizontalInput;
    private float verticalInput;
    private bool braking;

    void Start()
    {
        rigid.centerOfMass = new Vector3(0, -0.5f, 0); // improves stability
        GetComponent<Rigidbody>().centerOfMass = new Vector3(0, -0.6f, 0);
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        braking = Input.GetKey(KeyCode.Space);
    }

    void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleBraking();
        LimitSpeed();
    }

    void HandleMotor()
    {
        float motorPower = verticalInput * driveSpeed;

        rearLeft.motorTorque = motorPower;
        rearRight.motorTorque = motorPower;
    }

    void HandleSteering()
    {
        float steerAngle = steerSpeed * horizontalInput;

        frontLeft.steerAngle = steerAngle;
        frontRight.steerAngle = steerAngle;
    }

    void HandleBraking()
    {
        float brake = braking ? brakeForce : 0f;

        frontLeft.brakeTorque = brake;
        frontRight.brakeTorque = brake;
        rearLeft.brakeTorque = brake;
        rearRight.brakeTorque = brake;
    }

    void LimitSpeed()
    {
        if (rigid.velocity.magnitude > maxSpeed)
            rigid.velocity = rigid.velocity.normalized * maxSpeed;
    }
}