using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="MOD_FB_",menuName ="Magic/Fire/BallMod")]
public class Mod_FB : Modifier
{
    [Header("Specific")]
    public float _Size;
    public Material _Material;
    public bool _Weak = false;
    public bool _Ammo = false;
    public int _AmmoAmount = 0;
    public bool _Burn = false;
    public bool _Charge = false;
    public bool _Explosive = false;
    public bool _Curve = false;
    public float _Lifetime = 0;
    public int _Damage = 0;
    public float _SpeedBoost = 1;
    public int _Amount;
    public float _Spread = 1;
}
