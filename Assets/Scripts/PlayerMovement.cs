using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    Rigidbody _rb;

    [SerializeField] private float _Friction;
    [Header("Walking")]
    [SerializeField] private float _MaxSpeed;
    [SerializeField] private float _AccelWalk;
    [SerializeField] private float _MaxAccel;
    [SerializeField] private AnimationCurve _DirectionScaler;

    [Header("Sprinting")]
    [SerializeField] private float _MaxSpeedSprint;
    [SerializeField] private float _AccelSprint;
    [SerializeField] private float _MaxAccelSprint;
    [SerializeField] private AnimationCurve _DirectionScalerSprint;

    [Header("Mouse")]
    [SerializeField] private float _LookSpeed;
    [SerializeField] private float _LookLimit;

    [Header("Upforces")]
    [SerializeField] private float _HoverHeigt;
    [Range(0,1)][SerializeField] private float _HoverMinHeight;
    [SerializeField] private float _ForceMultiplier;
    [SerializeField] private float _JumpForce;
    [SerializeField] private float _HoverMaxForce;
    [SerializeField] private float _GravityForce;
    [SerializeField] private AnimationCurve _HoverForceScale;

    private float rotationX = 0;
    private Transform CamTran;
    private bool queuedJump;
    private bool Sprinting = false;

    void Start()
    {
        _JumpForce *= _ForceMultiplier;
        _HoverMaxForce *= _ForceMultiplier;
        _GravityForce *= _ForceMultiplier;

        CamTran = Camera.main.transform;
        _rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleMouse();
        if(Input.GetKeyDown(KeyCode.Space))
            queuedJump = true;
        if(Input.GetKeyUp(KeyCode.Space))
            queuedJump = false;
        if(Input.GetKey(KeyCode.LeftShift))
            Sprinting = true;
        else
            Sprinting = false;
    }

    void FixedUpdate()
    {
        HorizontalMovement();
        VerticalForces();
    }

    void HorizontalMovement()
    {
        Vector3 Wishdir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Wishdir = (transform.forward * Wishdir.z) + (transform.right * Wishdir.x);
        if (Wishdir.magnitude > 1.0f)
            Wishdir.Normalize();
        
        Vector3 vel = _rb.velocity;

        //dot for snappier movement here
        float dotOfVectors = Vector3.Dot(vel, Wishdir);
        float accel = _DirectionScaler.Evaluate(dotOfVectors) * (Sprinting ? _AccelSprint : _AccelWalk);
        //accel * factor here

        Vector3 goalVel = Wishdir * (Sprinting ? _MaxSpeedSprint : _MaxSpeed);

        Vector3 GoalVelocity = Vector3.MoveTowards(vel, goalVel, accel * Time.fixedDeltaTime);


        Vector3 Diff = (GoalVelocity - vel) / Time.fixedDeltaTime;
        Diff = Vector3.ClampMagnitude(Diff, Sprinting ? _MaxAccelSprint : _MaxAccel);

        _rb.AddForce(Diff * _rb.mass, ForceMode.Force);

        Friction();
    }

    void VerticalForces()
    {
        Vector3 startPoint = transform.position + new Vector3(0, -0.98f, 0);
        Ray ray = new Ray(startPoint,-transform.up);
        if(Physics.Raycast(ray, out RaycastHit _hit, _HoverHeigt))
        {
            float distanceScale = 1 - ((startPoint.y - _hit.point.y) / _HoverHeigt);
            float force = _HoverForceScale.Evaluate(distanceScale) * _HoverMaxForce;

            if (distanceScale > _HoverMinHeight && _rb.velocity.y < 0)
                _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

            if (force > _GravityForce)
                _rb.AddForce(new Vector3(0, force, 0));

            if (queuedJump)
            {
                _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
                _rb.AddForce(new Vector3(0,_JumpForce,0),ForceMode.Impulse);
                queuedJump = false;
            }
        }

        _rb.AddForce(new Vector3(0, -_GravityForce, 0));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position + new Vector3(0, -1, 0), transform.position + new Vector3(0, -1 - _HoverHeigt, 0));
    }

    void HandleMouse()
    {
        rotationX += -Input.GetAxis("Mouse Y") * _LookSpeed * Time.deltaTime;
        rotationX = Mathf.Clamp(rotationX, -_LookLimit, _LookLimit);

        //print(rotationX);

        CamTran.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * _LookSpeed * Time.deltaTime, 0);
    }

    void Friction()
    {
        _rb.AddForce(-_rb.velocity.normalized * _Friction);
    }
}
