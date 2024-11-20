using UnityEngine;
using System.Collections;
using Sirenix;
using Sirenix.OdinInspector;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using System.Collections.Generic;







public class BiteAttack : AbstractCreatureAttack
{
    //Declarations
    
    [TabGroup("General Settings", "Damage & Casting")]
    [SerializeField] protected float _atkRadius;
    [TabGroup("General Settings", "Damage & Casting")]
    [SerializeField] protected Color _biteAreaGizmoColor = Color.red;
    [TabGroup("Attack Features")]
    [SerializeField] protected bool _doesAtkPierceMultipleEntities = false;
    [TabGroup("Attack Features")]
    [SerializeField] [ReadOnly] protected bool _isAtkCastSatisfied = false;
    [BoxGroup("Debugging")]
    [SerializeField] [ReadOnly] protected Dictionary<int, int> _hitIDsDuringCast = new();



    //Monobehaviours
    private void Update()
    {
        //are we in the CastAtk state && have we NOT reached our hit limit
        if (_atkState == AtkState.castingAtk && !_isAtkCastSatisfied)
            CaptureDetectionsAndApplyDamage();
    }




    //Internals
    protected override void DrawAttackGizmos()
    {
        Gizmos.color = _biteAreaGizmoColor;
        Gizmos.DrawWireSphere(_atkOrigin.position, _atkRadius);
    }

    protected void CaptureDetectionsAndApplyDamage()
    {
        Debug.Log("Detecting...");

        //Cast a sphere of detection
        Collider[] currentDetections = Physics.OverlapSphere(_atkOrigin.position, _atkRadius, _detectionMask);

        foreach (Collider collider in currentDetections)
        {
            //Get the entity's identification
            IEntityID detectionIdentity = collider.GetComponent<IEntityID>();
            

            if (detectionIdentity != null) 
            {
                Debug.Log($"Detected Entity: {detectionIdentity.GetGameObject()}");
                //make sure the target is valid:
                //is entity of a different faction?
                //is entity alive?
                //has entity not already been hit by us?
                //is entity NOT US

                if ( detectionIdentity.GetFaction() != _damageInfo.AttackerFaction &&
                     !detectionIdentity.IsDead() &&
                     !_hitIDsDuringCast.ContainsKey(detectionIdentity.GetEntityID()) &&
                     detectionIdentity.GetEntityID() != _damageInfo.AttackerID)
                {
                    //Get the entity's healthbehavior
                    IHealthBehavior healthBehavior = collider.GetComponent<IHealthBehavior>();

                    if (healthBehavior != null) 
                    { 
                        //make sure the target isn't invincible.
                        //It shouldn't be marked as hit if it is invincible
                        if (!healthBehavior.IsInInvincRecovery())
                        {
                            Debug.Log($"Validation Success on {detectionIdentity.GetGameObject()}");

                            //mark the target as hit. Track the damage done to it
                            _hitIDsDuringCast[detectionIdentity.GetEntityID()] = _damageInfo.Damage;

                            //update our damageInfo's attack direciton
                            _damageInfo.SourceDirection = healthBehavior.GetGameObject().transform.position - transform.position;

                            //deal damage to the target
                            healthBehavior.TakeDamage(_damageInfo);

                            //Stop hitting entities if we cant pierce multiple enemies
                            if (!_doesAtkPierceMultipleEntities)
                            {
                                _isAtkCastSatisfied = true;
                                break;
                            }
                                
                        }
                    }
                }
            }
        }
    }

    protected override void PerformAdditionalUtilsOnAtkCastEntered()
    {
        //clear the previous hit data
        _hitIDsDuringCast.Clear();
        _isAtkCastSatisfied = false;
    }

    //Externals






}
