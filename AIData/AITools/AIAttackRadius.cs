using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SphereCollider))]
public class AIAttackRadius : MonoBehaviour
{
    public delegate void AttackEvent(IAttackable Target);    
    public AttackEvent onAttack;
    internal float attackDelay;
    internal SphereCollider rangeCollider;
    internal LayerMask environmentLayerMask;
    internal LayerMask attackLayerMask;
    internal Vector3 controllerOffset;
    private List<IAttackable> attackables = new List<IAttackable>();
    private Coroutine attackCoroutine;
    internal void Initalize()
    {
        rangeCollider = GetComponent<SphereCollider>();
        rangeCollider.excludeLayers = ~attackLayerMask;
    }

    private void OnTriggerEnter(Collider other) 
    {
        IAttackable _attackable = other.GetComponent<IAttackable>();
        if (_attackable != null)
        {
            if (_attackable.IsAttackable)
            {
                Ray ray = new Ray(transform.position + controllerOffset, (other.transform.position - (transform.position + controllerOffset)).normalized);
                if (Physics.Raycast(ray, rangeCollider.radius, ~environmentLayerMask))
                {
                        Debug.Log("Can attack");
                        AddAttackable(_attackable);
                }
            }
        }
    }
    public void AddAttackable(IAttackable _attackable)
    {
        attackables.Add(_attackable);
        if (attackCoroutine == null)
        {
            attackCoroutine = StartCoroutine(Attack());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        IAttackable damageable = other.GetComponent<IAttackable>();
        if (damageable != null)
        {
            attackables.Remove(damageable);
            if (attackables.Count == 0)
            {
                if (attackCoroutine != null)
                {
                    StopCoroutine(attackCoroutine);
                    attackCoroutine = null;
                }
            }
        }
    }
    private IEnumerator Attack()
    {
        WaitForSeconds Wait = new WaitForSeconds(attackDelay);

        IAttackable closestDamageable = null;
        float closestDistance = float.MaxValue;

        while (attackables.Count > 0)
        {
            for (int i = 0; i < attackables.Count; i++)
            {
                Transform damageableTransform = attackables[i].GetTransform();
                float distance = Vector3.Distance(transform.position, damageableTransform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDamageable = attackables[i];
                }
            }

            if (closestDamageable != null)
            {
                onAttack?.Invoke(closestDamageable);
                closestDamageable.TakeHit();

            }

            closestDamageable = null;
            closestDistance = float.MaxValue;

            yield return Wait;

            attackables.RemoveAll(DisabledAttackables);
        }

        attackCoroutine = null;
    }
    private bool DisabledAttackables(IAttackable _attackable)
    {
        return _attackable != null && !_attackable.GetTransform().gameObject.activeSelf;
    }
}