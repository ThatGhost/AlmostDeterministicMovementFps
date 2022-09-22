using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_FB : MonoBehaviour
{
    private int _Damage;
    public int Damage
    {
        set { _Damage = value; }
    }

    private bool _Explosive;
    public bool Explosive
    {
        set { _Explosive = value; }
    }

    private bool _Burn;
    public bool Burn
    {
        set { _Burn = value; }
    }

    private float _LifeTime;
    public float LifeTime
    {
        set { _LifeTime = value; }
    }

    private void OnEnable()
    {
        StartCoroutine(KillMe(_LifeTime));
    }

    private IEnumerator KillMe(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Player" && !other.isTrigger)
        {
            IDamagable damagable = other.GetComponent<IDamagable>();
            if(damagable != null)
            {
                damagable.Damage(_Damage);
            }

            gameObject.SetActive(false);
        }
    }
}
