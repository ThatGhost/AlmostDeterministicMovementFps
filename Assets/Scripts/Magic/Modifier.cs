using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Modifier : ScriptableObject
{
    [Header("General")]
    public string Name;
    public string Description;

    public int ManaCost;
    public float CooldownCost;
}
