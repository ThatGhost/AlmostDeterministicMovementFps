using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class PlayerController : MonoBehaviour
{
    Rigidbody _rb;
    CapsuleCollider _coll;

    public int ForceScale
    { get { return _ForceScale; } }

    [Header("General")]
    [SerializeField] private int _ForceScale;
    [SerializeField] private int _Friction;
    [SerializeField] private int _Gravity;
    [SerializeField] private int _MaxDownwardsSpeed;
    [SerializeField] private Transform _Feet;
    [SerializeField] private float _FeetDistance;

    [Header("Mouse")]
    [SerializeField] private float _LookSpeed;
    [SerializeField] private float _LookLimit;

    [Header("Hover")]
    [SerializeField] private int _HoverHeight;
    [SerializeField] private int _StandLimit;
    [SerializeField] private int _HoverMinHeight;
    [SerializeField] private int _HoverForce;
    [SerializeField] private int _HoverMinForce;
    [SerializeField] private int _HoverMaxForce;
    [SerializeField] private AnimationCurve _HoverForceMultiplier;

    [Header("Jumping")]
    [SerializeField] private int _JumpForce;

    [Header("Walking")]
    [SerializeField] private int _WalkingForce;
    [SerializeField] private int _WalkingMaxSpeed;
    [SerializeField] private int _WalkingAccel;
    [SerializeField] private int _WalkingDecel;
    [SerializeField] private AnimationCurve _WalkingSnappyness;

    [Header("Running")]
    [SerializeField] private int _RunningForce;
    [SerializeField] private int _RunningMaxSpeed;
    [SerializeField] private int _RunningAccel;
    [SerializeField] private int _RunningDecel;
    [SerializeField] private AnimationCurve _RunningSnappyness;

    [Header("Crouching")]
    [SerializeField] private int _CrouchingFactor;

    private IntVector3D _velocity = IntVector3D.zero;
    private IntVector3D _position = IntVector3D.zero;
    private float _RotationX=0;
    private Transform _CamTran;
    
    private Deque<uint> _InputQueue = new Deque<uint>();
    private bool _Grounded = false;
    private bool _Jumping = false;
    private bool _Crouching = false;
    private bool _Loose = false;
    private Vector3 _ScaleVector;

    private Dictionary<GameObject, ContactPoint[]> _Collisions = new Dictionary<GameObject, ContactPoint[]>();
    private List<KeyValuePair<IntVector3D, ForceMode>> _AdditiveForcesBuffer = new List<KeyValuePair<IntVector3D, ForceMode>>();

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _rb = GetComponent<Rigidbody>();
        _coll = GetComponent<CapsuleCollider>();
        _CamTran = Camera.main.transform;
        _ScaleVector = new Vector3(1f / _ForceScale, 1f / _ForceScale, 1f / _ForceScale);
        Vector3 currentpos = transform.position;
        _position = new IntVector3D((int)(currentpos.x * _ForceScale), (int)(currentpos.y * _ForceScale), (int)(currentpos.z * _ForceScale));        
    }

    private void Update()
    {
        HandleMouse();
        RecordInput();
    }

    private void FixedUpdate()
    {
        while (_InputQueue.Count > 0)
        {
            Movement(_InputQueue.RemoveFromBack());
        }
        NextPhysicsStep();
        ApplyPosition();
    }

    private void Movement(uint input)
    {
        //translate input
        Vector3 inputdir = new Vector3(((input & (uint)InputType.Left)!= 0 ? -1 : 0) + ((input & (uint)InputType.Right) != 0 ? 1 : 0)
                                        ,0,
                                      ((input & (uint)InputType.Down) != 0 ? -1 : 0) + ((input & (uint)InputType.Up) != 0 ? 1 : 0));
        inputdir = (transform.forward * inputdir.z) + (transform.right * inputdir.x);

        bool crouching = (input & (uint)InputType.Crouching) != 0;
        bool running = (input & (uint)InputType.Running) != 0;

        if(!crouching && _Crouching)
        {
            Ray ray = new Ray(transform.position, Vector3.up);
            if (Physics.Raycast(ray, 1.2f))
            {
                crouching = true;
            }
            else
            {
                _coll.height = 2;
                _coll.center += new Vector3(0,0.5f,0);
                _CamTran.position += new Vector3(0, 0.5f, 0);
            }
        }
        else if(crouching && !_Crouching)
        {
            _coll.height = 1;
            _coll.center += new Vector3(0, -0.5f, 0);
            _CamTran.position += new Vector3(0,-0.5f,0);
        }

        _Crouching = crouching;

        //making wish direction
        IntVector3D WishDir = new IntVector3D((int)(inputdir.x * _ForceScale), 0, (int)(inputdir.z * _ForceScale));

        if (WishDir.magnitude() > _ForceScale)
            WishDir = WishDir.normilized();
        WishDir *= crouching ? (!running ? _WalkingForce : _RunningForce) / _CrouchingFactor : !running ? _WalkingForce : _RunningForce;

        //Debug.Log($"wishdir: {WishDir}, velocity {_velocity}, eval {_RunningSnappyness.Evaluate(_velocity.flattened.normilized().dot(WishDir.normilized()) / _ForceScale)}, dot {_velocity.flattened.normilized().dot(WishDir.normilized())/_ForceScale}");

        //acceliration - decceliration
        IntVector3D newVel;
        if (WishDir != IntVector3D.zero)
        {
            if (crouching)
            {
                newVel = _velocity.flattened.MoveTowards(WishDir, (!running ? _WalkingAccel : _RunningAccel)*_CrouchingFactor, _ForceScale);

            }
            else if (running)
            {
                newVel = _velocity.flattened.MoveTowards(WishDir, _RunningAccel, _ForceScale);

            }
            else
            {
                newVel = _velocity.flattened.MoveTowards(WishDir, _WalkingAccel, _ForceScale);
            }

            //(int)(running ?
            //_RunningSnappyness.Evaluate(_velocity.flattened.normilized().dot(WishDir.normilized()) / _ForceScale) :
            //_RunningSnappyness.Evaluate(_velocity.flattened.normilized().dot(WishDir.normilized()) / _ForceScale))
            ///_ForceScale);
        }
        else
        {
            if (crouching)
            {
                newVel = _velocity.flattened.MoveTowards(IntVector3D.zero, (!running ? _WalkingDecel : _RunningDecel)/_CrouchingFactor, _ForceScale);

            }
            else if (running)
            {
                newVel = _velocity.flattened.MoveTowards(IntVector3D.zero, _RunningDecel, _ForceScale);

            }
            else
            {
                newVel = _velocity.flattened.MoveTowards(IntVector3D.zero, _WalkingDecel, _ForceScale);
            }

            //(int)(running ?
            //_RunningSnappyness.Evaluate(_velocity.flattened.normilized().dot(WishDir.normilized()) / _ForceScale) :
            //_RunningSnappyness.Evaluate(_velocity.flattened.normilized().dot(WishDir.normilized()) / _ForceScale))
            /// _ForceScale);
        }

        _velocity = new IntVector3D( newVel.x, _velocity.y, newVel.z);

        if(_Grounded && !_Loose)
        {
            if (crouching)
            {
                if (_velocity.magnitude() > ((running ? _RunningMaxSpeed : _WalkingMaxSpeed) / _CrouchingFactor))
                {
                    _velocity.AddAsFlattened(_velocity.flattened.normilized() * ((running ? _RunningMaxSpeed : _WalkingMaxSpeed) / _CrouchingFactor));
                }
            }
            else if (running)
            {
                if (_velocity.magnitude() > _RunningMaxSpeed)
                {
                    _velocity.AddAsFlattened(_velocity.flattened.normilized() * _RunningMaxSpeed);
                }
            }
            else
            {
                if (_velocity.magnitude() > _WalkingMaxSpeed)
                {
                    _velocity.AddAsFlattened(_velocity.flattened.normilized() * _WalkingMaxSpeed);
                }
            }
        }

        //Jump
        if ((input & (uint)InputType.JumpDown) != 0 && _Grounded)
        {
            _velocity.y = _JumpForce;
            _Jumping = true;
        }
    }

    private void NextPhysicsStep()
    {
        AddForces();
        CheckCollisions();
        VerticalPhysics();
        _position += (_velocity / 40);
    }

    private void CheckCollisions()
    {
        foreach (KeyValuePair<GameObject, ContactPoint[]> pair in _Collisions)
        {
            foreach(ContactPoint point in pair.Value)
            {
                IntVector3D normal = new IntVector3D((int)(point.normal.x * _ForceScale), 0, (int)(point.normal.z * _ForceScale));
                IntVector3D surface = new IntVector3D(normal.z, 0,-normal.x);

                if(_velocity.dot(normal) < 0)
                {
                    IntVector3D newVel = IntVector3D.Project(_velocity.flattened, surface);
                    _velocity = new IntVector3D(newVel.x,_velocity.y,newVel.z);
                }
            }
        }
    }

    private void VerticalPhysics()
    {
        _velocity.y -= _Gravity;

        if(_velocity.y < _MaxDownwardsSpeed)
            _velocity.y = _MaxDownwardsSpeed;
        if(_velocity.y > -_MaxDownwardsSpeed && !_Loose)
            _velocity.y = -_MaxDownwardsSpeed;

        //push character upwards
        Ray[] rays = new Ray[]{ 
            new Ray(_Feet.position + new Vector3(0,0,0), Vector3.down),
            new Ray(_Feet.position + new Vector3(_FeetDistance,0,0), Vector3.down),
            new Ray(_Feet.position + new Vector3(-_FeetDistance,0,0), Vector3.down),
            new Ray(_Feet.position + new Vector3(0,0,-_FeetDistance), Vector3.down),
            new Ray(_Feet.position + new Vector3(0,0,_FeetDistance), Vector3.down)
        };
        _Grounded = false;
        if ((Physics.Raycast(rays[0], out RaycastHit hit, _HoverHeight / (float)_ForceScale) ||
            Physics.Raycast(rays[1], out hit, _HoverHeight / (float)_ForceScale) ||
            Physics.Raycast(rays[2], out hit, _HoverHeight / (float)_ForceScale) ||
            Physics.Raycast(rays[3], out hit, _HoverHeight / (float)_ForceScale) ||
            Physics.Raycast(rays[4], out hit, _HoverHeight / (float)_ForceScale))
            && !hit.collider.isTrigger)
        {
            _Grounded = true;

            if(_Loose && _velocity.y < 0)
                _Loose = false;

            int distance = IntVector3D.Convert(_Feet.position, _ForceScale).y - IntVector3D.Convert(hit.point, _ForceScale).y;
            distance /= _HoverHeight/_ForceScale;

            if((distance < _HoverMinHeight && _velocity.y < 0) || (_velocity.flattened.magnitude() < _StandLimit && !_Jumping && !_Loose))
            {
                _velocity.y = 0;
            }

            int pushForce = (int)(_HoverForceMultiplier.Evaluate(distance) / _ForceScale * _HoverForce);
            if(_velocity.y < 0)
            {
                pushForce += (int)(_HoverForceMultiplier.Evaluate(distance) / _ForceScale * _HoverForce);
            }

            if(pushForce > _HoverMaxForce)
                pushForce = _HoverMaxForce;

            if(pushForce > _HoverMinForce)
                _velocity.y += pushForce;
        }
        else
        {
            if(_Jumping)
                _Jumping = false;
        }

        //Stop going up when colliding with roof
        Ray ray = new Ray(transform.position + new Vector3(0,1,0), Vector3.up);
        if(Physics.Raycast(ray, out RaycastHit hit2,0.2f) && _velocity.y > 0 && !hit2.collider.isTrigger)
        {
            _velocity.y = 0;
        }
    }

    private void ApplyPosition()
    {
        _rb.position = Vector3.Scale(new Vector3(_position.x, _position.y, _position.z), _ScaleVector);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!collision.collider.isTrigger)
            _Collisions.Add(collision.gameObject, collision.contacts);
    }
    private void OnCollisionExit(Collision collision)
    {
        if (!collision.collider.isTrigger && _Collisions.ContainsKey(collision.gameObject))
            _Collisions.Remove(collision.gameObject);
    }

    private void RecordInput()
    {
        uint input = 0;
        if (Input.GetMouseButton(0))
            input |= (UInt16)InputType.Ability1;
        if (Input.GetKey(KeyCode.E))
            input |= (UInt16)InputType.Ability2;
        if (Input.GetKey(KeyCode.V))
            input |= (UInt16)InputType.Ability3;
        if (Input.GetKey(KeyCode.C))
            input |= (UInt16)InputType.Ability4;
        if (Input.GetKey(KeyCode.Q))
            input |= (UInt16)InputType.Ultimate;

        if (Input.GetKey(KeyCode.W))
            input |= (UInt16)InputType.Up;
        if (Input.GetKey(KeyCode.A))
            input |= (UInt16)InputType.Left;
        if (Input.GetKey(KeyCode.S))
            input |= (UInt16)InputType.Down;
        if (Input.GetKey(KeyCode.D))
            input |= (UInt16)InputType.Right;

        if (Input.GetKey(KeyCode.LeftShift))
            input |= (UInt16)InputType.Running;
        if (Input.GetKey(KeyCode.LeftControl))
            input |= (UInt16)InputType.Crouching;
        if (Input.GetKeyUp(KeyCode.Space))
            input |= (UInt16)InputType.JumpUp;
        if (Input.GetKeyDown(KeyCode.Space))
            input |= (UInt16)InputType.JumpDown;
        if (Input.GetKey(KeyCode.E))
            input |= (UInt16)InputType.Interact;
        _InputQueue.AddToFront(input);
    }

    private void HandleMouse()
    {
        _RotationX += -Input.GetAxis("Mouse Y") * _LookSpeed * Time.deltaTime;
        _RotationX = Mathf.Clamp(_RotationX, -_LookLimit, _LookLimit);

        _CamTran.localRotation = Quaternion.Euler(_RotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * _LookSpeed * Time.deltaTime, 0);
    }

    private void AddForces()
    {
        foreach(KeyValuePair<IntVector3D,ForceMode> pair in _AdditiveForcesBuffer)
        {
            switch (pair.Value)
            {
                case ForceMode.Force:
                    _velocity += pair.Key;
                    break;
                case ForceMode.Impulse:
                    _velocity = pair.Key;
                    
                    _Loose = true;
                    break;
                default:
                    Debug.LogError("ForceMode type not Defined");
                    break;
            }
        }
        _AdditiveForcesBuffer.Clear();
    }

    public void AddForce(IntVector3D force, ForceMode forcemode)
    {
        _AdditiveForcesBuffer.Add(new KeyValuePair<IntVector3D,ForceMode>(force,forcemode));
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //bottom
        Gizmos.DrawLine(_Feet.transform.position + new Vector3(0,0,0), _Feet.transform.position + new Vector3(0,0,0) + (Vector3.down*(_HoverHeight/(float)_ForceScale)));
        Gizmos.DrawLine(_Feet.transform.position + new Vector3(_FeetDistance,0,0), _Feet.transform.position + new Vector3(_FeetDistance, 0,0) + (Vector3.down*(_HoverHeight/(float)_ForceScale)));
        Gizmos.DrawLine(_Feet.transform.position + new Vector3(-_FeetDistance, 0,0),_Feet.transform.position + new Vector3(-_FeetDistance, 0,0) + (Vector3.down*(_HoverHeight/(float)_ForceScale)));
        Gizmos.DrawLine(_Feet.transform.position + new Vector3(0,0, _FeetDistance), _Feet.transform.position + new Vector3(0,0, _FeetDistance) + (Vector3.down*(_HoverHeight/(float)_ForceScale)));
        Gizmos.DrawLine(_Feet.transform.position + new Vector3(0,0,-_FeetDistance),_Feet.transform.position + new Vector3(0, 0, -_FeetDistance) + (Vector3.down*(_HoverHeight/(float)_ForceScale)));

        //head
        Gizmos.DrawLine(transform.position + new Vector3(0, _FeetDistance, 0),transform.position + new Vector3(0,1.5f,0));
        if(_Crouching)
            Gizmos.DrawLine(transform.position + new Vector3(0.2f, 0, 0.2f), transform.position + new Vector3(0.2f, 0, 0.2f) + Vector3.up);
    }

#endif
}

[Flags]
public enum InputType : uint
{
    Ultimate = 1,
    Ability1 = 2,
    Ability2 = 4,
    Ability3 = 8,

    Ability4 = 16,
    Emote = 32,
    Left = 64,
    Right = 128,

    Up = 256,
    Down = 512,
    Running = 1024,
    JumpDown = 2048,

    JumpUp = 4096,
    Crouching = 8192,
    Aiming = 16_384,
    Interact = 32_768,
}

[Serializable]
public struct IntVector3D
{
    public int x;
    public int y;
    public int z;

    public IntVector3D(int x, int y, int z)
    {
        this.x = x; this.y = y; this.z = z;
    }

    public static IntVector3D zero { get { return new IntVector3D(); } }
    public static IntVector3D up { get { return new IntVector3D(0,1000,0); } }
    public static IntVector3D forward { get { return new IntVector3D(0,0,1000); } }
    public static IntVector3D right { get { return new IntVector3D(1000,0,0); } }
    public int magnitude ()
    {
        return (int)Mathf.Sqrt((x * x) + (y * y) + (z * z));
    }
    public IntVector3D normilized ()
    {
        IntVector3D result = new IntVector3D(x * 1000, y * 1000, z * 1000);
        int mag = magnitude();
        if (mag != 0)
            return new IntVector3D(result.x / mag, result.y / mag, result.z / mag);
        else
            return IntVector3D.zero;
    }

    public static IntVector3D Convert(Vector3 vector, int scale)
    {
        return new IntVector3D((int)(vector.x * scale), (int)(vector.y * scale), (int)(vector.z * scale));
    }

    public IntVector3D MoveTowards(IntVector3D other, int stepsize, int scale)
    {
        return this + (((other - this).normilized() * stepsize)/scale);
    }

    public static IntVector3D operator +(IntVector3D lhs, IntVector3D rhs)
    {
        lhs.x += rhs.x;
        lhs.y += rhs.y;
        lhs.z += rhs.z;
        return lhs;
    }
    public static IntVector3D operator -(IntVector3D lhs, IntVector3D rhs)
    {
        lhs.x -= rhs.x;
        lhs.y -= rhs.y;
        lhs.z -= rhs.z;
        return lhs;
    }
    public static bool operator ==(IntVector3D lhs, IntVector3D rhs)
    {
        return lhs.x == rhs.x && lhs.y == rhs.y && lhs.z == rhs.z;
    }
    public static bool operator !=(IntVector3D lhs, IntVector3D rhs)
    {
        return lhs.x != rhs.x || lhs.y != rhs.y || lhs.z != rhs.z;
    }

    public static IntVector3D operator *(IntVector3D lhs,int rhs)
    {
        return new IntVector3D(lhs.x * rhs,lhs.y * rhs, lhs.z*rhs);
    }
    public int dot(IntVector3D other)
    {
        return x * other.x + y * other.y + z * other.z;
    }
    public static IntVector3D operator /(IntVector3D lhs,int rhs)
    {
        return new IntVector3D(lhs.x / rhs,lhs.y / rhs, lhs.z/rhs);
    }
    public static IntVector3D Project(IntVector3D from,IntVector3D onto)
    {
        return onto * (from.dot(onto) / onto.dot(onto));
    }
    public IntVector3D flattened { get { return new IntVector3D(x, 0, z); } }
    public IntVector3D AddAsFlattened(IntVector3D other)
    {
        return new IntVector3D(x+other.x,y,z+other.z);
    }

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }
    public override bool Equals(object obj)
    {
        return this == (IntVector3D)obj;
    }
    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}