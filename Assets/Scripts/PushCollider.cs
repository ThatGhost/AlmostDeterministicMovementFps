using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushCollider : MonoBehaviour
{
    [SerializeField] private Vector3 _ConstantForce;
    [SerializeField] private string _Tag;
    private Dictionary<GameObject, PlayerController> _rbs = new Dictionary<GameObject, PlayerController>();

    private void FixedUpdate()
    {
        foreach(KeyValuePair<GameObject,PlayerController> pair in _rbs)
        {
            pair.Value.AddForce(IntVector3D.Convert(_ConstantForce,1),ForceMode.Force);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == _Tag && !_rbs.ContainsKey(other.gameObject))
        {
            _rbs.Add(other.gameObject,other.GetComponent<PlayerController>());
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
