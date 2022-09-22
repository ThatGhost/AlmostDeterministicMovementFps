using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defaults : ScriptableObject
{
    [Header("General")]
    public float _ChargeTime;
    public float _CooldownTime;
    public int _ManaCost;
    public RuntimeAnimatorController _ArmsAnimatorController;
}
