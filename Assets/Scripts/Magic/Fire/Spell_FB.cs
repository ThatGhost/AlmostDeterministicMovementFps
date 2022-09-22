using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Spell_FB : SpellBehaviour
{
    private Transform _attackPoint;
    private Default_FB default_;
    private Animator _animator;

    //specific things
    //ammo
    private int _storedAmmo = 0;
    private Coroutine _ammoTimer;

    private void Awake()
    {
        default_ = Instantiate(Resources.Load<Default_FB>("Magic/Fire/Default_FB"));
        _attackPoint = GetComponent<SpellController>().AttackPoint;
        _animator = GetComponent<Animator>();
        _animator.runtimeAnimatorController = default_._ArmsAnimatorController;
    }

    public override void ModMyself(SpellController controller)
    {
        _Controller = controller;

        foreach (var item in _Modifiers)
        {

            Mod_FB mods = (Mod_FB)item;
            default_._CooldownTime += item.CooldownCost;
            default_._ManaCost += item.ManaCost;
            default_._Size += mods._Size;
            default_._Ammo = mods._Ammo;
            if (default_._Ammo)
            {
                _animator.speed = 2;
                default_._AmmoAmount = mods._AmmoAmount;
            }
            default_._Burn = mods._Burn; 
            default_._Charge = mods._Charge;
            default_._Explosive = mods._Explosive;
            default_._Curve = mods._Curve;
            if (mods._Lifetime != 0)
            {
                default_._LifeTime = mods._Lifetime;
            }
            default_._Damage += mods._Damage;
            default_._Speed *= mods._SpeedBoost;
            default_._Amount += mods._Amount;
            default_._Spread *= mods._Spread;

            if (mods._Material != null)
                default_._material = mods._Material;
            if (mods._Weak)
                default_._Weak = true;
        }
    }

    public override void TriggerAnimationComeBack()
    {
        Shoot();
    }

    public override void ActivateSingle()
    {
        if(!default_._Weak)
        {
            //stored ammo behaviour
            if (default_._Ammo)
            {
                if(_storedAmmo > 0 && _Controller.RegisterFire(this))
                {
                    _storedAmmo--;
                    _animator.SetTrigger("Shoot");
                }
                return;
            }

            //default behaviour
            if (_Active && _Controller.HasMana(default_._ManaCost)  && _Controller.RegisterFire(this))
            {
                _animator.SetTrigger("Shoot");
                StartCoolDown(default_._CooldownTime);
                _Controller.DrainMana(default_._ManaCost);
                return;
            }
        }
    }

    public override void EnableLong()
    {
        if(default_._Weak)
        {
            _animator.SetBool("Long",true);
        }
        if(default_._Ammo)
            _ammoTimer = StartCoroutine(LoadAmmo());
    }

    public override void Long()
    {
        //weak
        if (default_._Weak && _Active && _Controller.HasMana(default_._ManaCost))
        {
            StartCoolDown(default_._CooldownTime);
            _Controller.DrainMana(default_._ManaCost);
            Shoot();
        }
    }

    public override void DisableLong()
    {
        if (default_._Weak)
        {
            _animator.SetBool("Long", false);
        }
        if (default_._Ammo)
            StopCoroutine(_ammoTimer);
    }

    private void Shoot()
    {
        GameObject b = GetBullet();
        b.transform.position = _attackPoint.position;
        b.SetActive(true);

        Rigidbody rb = b.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.AddForce(_attackPoint.forward * default_._Speed, ForceMode.Impulse);

        if(default_._Amount > 1)
        {
            float degree = 0;
            float step = 360 / (default_._Amount - 1);
            float angle = transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

            for (int i = 0; i < default_._Amount-1; i++)
            {
                GameObject other = GetBullet();
                other.SetActive(true);

                Vector3 basePos = new Vector3(Mathf.Cos(Mathf.Deg2Rad * degree), Mathf.Sin(Mathf.Deg2Rad * degree),Random.value * default_._Spread * 2) * default_._Size * default_._Spread;
                basePos = new Vector3(basePos.x * Mathf.Cos(angle) + basePos.z * Mathf.Sin(angle),
                                      basePos.y,
                                      -basePos.x * Mathf.Sin(angle) + basePos.z * Mathf.Cos(angle));
                other.transform.position = _attackPoint.position + basePos;

                Rigidbody rb2 = other.GetComponent<Rigidbody>();
                rb2.velocity = Vector3.zero;
                Vector3 forward = _attackPoint.forward + new Vector3(Random.value * 0.1f, Random.value * 0.1f, Random.value * 0.1f);
                rb2.AddForce(forward * default_._Speed, ForceMode.Impulse);

                degree += step;
            }
        }
    }

    private GameObject GetBullet()
    {
        foreach (var poolObject in _pool)
        {
            if (!poolObject.activeSelf)
            {
                return poolObject;
            }
        }
        return MakeBullet();
    }

    private GameObject MakeBullet()
    {
        GameObject bullet = Instantiate(default_._FB, _attackPoint.position, Quaternion.identity);
        bullet.transform.localScale = new Vector3(default_._Size, default_._Size, default_._Size);
        //bullet.GetComponentInChildren<MeshRenderer>().material = new Material(default_._material);

        Projectile_FB projectile = bullet.GetComponent<Projectile_FB>();
        projectile.Damage = default_._Damage;
        projectile.Explosive = default_._Explosive;
        projectile.Burn = default_._Burn;
        projectile.LifeTime = default_._LifeTime;
        _pool.Add(bullet);
        return bullet;
    }

    private IEnumerator LoadAmmo()
    {
        yield return new WaitForSeconds(1);
        _storedAmmo++;
        _Controller.DrainMana(default_._ManaCost);

        if (_storedAmmo < default_._AmmoAmount && _Controller.HasMana(default_._ManaCost))
            _ammoTimer = StartCoroutine(LoadAmmo());
    }
}
