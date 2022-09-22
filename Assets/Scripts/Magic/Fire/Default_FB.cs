using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Default_FB", menuName = "Magic/defaults/FB")]
public class Default_FB : Defaults
{
    [Header("Specific")]
    public GameObject _FB;
    public Material _material;
    public float _Size = 1;
    public float _Speed = 50;
    public bool _Weak = false;
    public bool _Ammo;
    public int _AmmoAmount;
    public bool _Burn;
    public bool _Charge;
    public bool _Explosive;
    public bool _Curve;
    public float _LifeTime;
    public int _Damage;
    public int _Amount;
    public float _Spread;
}
