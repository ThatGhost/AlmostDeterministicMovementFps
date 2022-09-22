using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour, IDamagable
{
    private MeshRenderer _renderer;
    [SerializeField] private Material _materialRed;
    [SerializeField] private Material _materialGreen;
    bool _enabled = false;

    public void Damage(int hp)
    {
        if(!_enabled)
        {
            _renderer.material = new Material(_materialGreen);
            StartCoroutine(ResetColor());
            _enabled = true;
        }
    }

    void Start()
    {
        _renderer = GetComponent<MeshRenderer>();
        _renderer.material = new Material(_materialRed);
    }

    private IEnumerator ResetColor()
    {
        yield return new WaitForSeconds(3);
        _renderer.material = _materialRed;
        _enabled = false;
    }
}
