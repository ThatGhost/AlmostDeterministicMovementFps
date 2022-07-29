using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushCollider : MonoBehaviour
{
    [SerializeField] private Vector3 _ConstantForce;
    [SerializeField] private string _Tag;
    private Dictionary<GameObject,Rigidbody> _rbs = new Dictionary<GameObject, Rigidbody>();

    private void FixedUpdate()
    {
        foreach(KeyValuePair<GameObject,Rigidbody> pair in _rbs)
        {
            pair.Value.AddForce(_ConstantForce);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == _Tag && !_rbs.ContainsKey(other.gameObject))
        {
            _rbs.Add(other.gameObject,other.GetComponent<Rigidbody>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == _Tag)
        {
            _rbs.Remove(other.gameObject);
        }
    }
}
