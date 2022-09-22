using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinybox : MonoBehaviour
{
    Rigidbody _rb;
    [SerializeField] private float spiningPower;
    [SerializeField] private float spiningMaxPower;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.maxAngularVelocity = spiningMaxPower;
    }

    // Update is called once per frame
    void Update()
    {
        _rb.AddTorque(new Vector3(0, spiningPower, 0), ForceMode.Force);
    }
}
