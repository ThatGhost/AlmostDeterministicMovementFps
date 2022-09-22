using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellController : MonoBehaviour
{
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private Image _manaImage;
    [SerializeField] private SpellBehaviour[] _spells;

    private const int _SpellAmount = 3;
    private bool[] _wasFiring = new bool[_SpellAmount];
    protected SpellBehaviour _toTriggerSpell;

    private const int _MaxMana = 100;
    private int _Mana = _MaxMana;

    public Transform AttackPoint
    {
        get { return _attackPoint; }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Regen(0.1f,1));
        foreach (var item in _spells)
        {
            item.ModMyself(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //rightclick
        if (Input.GetMouseButtonUp(2))
            _spells[0].EnableAlternate();
        if (Input.GetMouseButtonDown(2))
            _spells[0].DisableAlternate();

        //left click
        if (Input.GetMouseButton(0))
        {
            if (!_wasFiring[0])
            {
                _spells[0].ActivateSingle();
                _spells[0].EnableLong();
            }
            _spells[0].Long();
            _wasFiring[0] = true;
        }
        else
        {
            if (_wasFiring[0])
            {
                _spells[0].DisableLong();
            }
            _wasFiring[0] = false;
        }

        //ability 1
        if (Input.GetKey(KeyCode.Q))
        {
            if (!_wasFiring[1])
            {
                _spells[1].ActivateSingle();
                _spells[1].EnableLong();
            }
            _spells[1].Long();
            _wasFiring[1] = true;
        }
        else
        {
            if (_wasFiring[1])
            {
                _spells[1].DisableLong();
            }
            _wasFiring[1] = false;
        }

        //ability 2
        if (Input.GetKey(KeyCode.E))
        {
            if (!_wasFiring[2])
            {
                _spells[2].ActivateSingle();
                _spells[2].EnableLong();
            }
            _spells[2].Long();
            _wasFiring[2] = true;
        }
        else
        {
            if (_wasFiring[2])
            {
                _spells[2].DisableLong();
            }
            _wasFiring[2] = false;
        }
    }

    public bool RegisterFire(SpellBehaviour spell)
    {
        if(_toTriggerSpell == null)
        {
            _toTriggerSpell = spell;
            return true;
        }
        return false;
    }

    public void TriggerFire()
    {
        _toTriggerSpell.TriggerAnimationComeBack();
        _toTriggerSpell = null;
    }

    private IEnumerator Regen(float interval, int amount)
    {
        yield return new WaitForSeconds(interval);
        DrainMana(-amount);
        StartCoroutine(Regen(interval,amount));
    }

    public void DrainMana(int mana)
    {
        int hereMana = _Mana - mana;
        _Mana = Mathf.Clamp(hereMana, 0, _MaxMana);
        _manaImage.fillAmount = _Mana / (float)_MaxMana;
    }

    public bool HasMana(int mana)
    {
        int hereMana = _Mana - mana;
        hereMana = Mathf.Clamp(hereMana, 0, _MaxMana);
        return hereMana != 0;
    }
}
