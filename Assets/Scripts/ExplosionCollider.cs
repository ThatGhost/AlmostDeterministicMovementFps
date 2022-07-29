using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionCollider : MonoBehaviour
{
    [SerializeField] private IntVector3D _Force;
    private void OnTriggerEnter(Collider other)
    {
        other.GetComponent<PlayerController>().AddForce(_Force,ForceMode.Impulse);
    }
}
