using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SpellBehaviour : MonoBehaviour
{
    [Header("General")]
    [SerializeField] protected Modifier[] _Modifiers;
    protected SpellController _Controller;
    protected bool _Active = true;
    private Coroutine _cooldownRoutine;
    protected List<GameObject> _pool = new List<GameObject>();

    [Header("Temp")]
    [SerializeField] private Image _fillImage;

    public abstract void ModMyself(SpellController controller);
    public virtual void ActivateSingle() { }
    public virtual void EnableLong() { }
    public virtual void Long() { }
    public virtual void DisableLong() { }
    public virtual void EnableAlternate() { }
    public virtual void DisableAlternate() { }
    public virtual void TriggerAnimationComeBack() { }

    protected void StartCoolDown(float time)
    {
        _cooldownRoutine = StartCoroutine(CoolDown(time));
    }

    protected void ResetCooldown()
    {
        StopCoroutine(_cooldownRoutine);
        _Active = true;
    }

    private IEnumerator CoolDown(float time)
    {
        _Active = false;
        float current = 0;
        while(current < time)
        {
            yield return new WaitForSeconds(0.1f);
            current += 0.1f;
            _fillImage.fillAmount = current / time;
        }
        _Active = true;
    }
}
