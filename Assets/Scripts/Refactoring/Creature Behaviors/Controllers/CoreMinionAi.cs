using UnityEngine;
using Sirenix;
using Sirenix.OdinInspector;
using System.Collections.Generic;





public class CoreMinionAi : AbstractCreatureBehavior
{
    //Declarations
    private IAiMoveBehavior _moveBehavior;
    private IAttackBehaviour _attackBehavior;
    private ICarryBehavior _carryBehavior;



    //Monobehaviours




    //Internals
    protected override void  InitializeCreatureReferences()
    {
        base.InitializeCreatureReferences();
        _moveBehavior = GetComponent<IAiMoveBehavior>();
        _attackBehavior = GetComponent<IAttackBehaviour>();
        _carryBehavior = GetComponent<ICarryBehavior>();
        
    }


    //Externals
    [TabGroup("Creature", "Debug")]
    [Button]
    public void MoveToPosition(Vector3 position)
    {
        _moveBehavior.MoveToLocation(position);
    }

    [TabGroup("Creature", "Debug")]
    [Button]
    public void ApproachObject(Transform target)
    {
        _moveBehavior.ApproachObject(target);
    }

    [TabGroup("Creature", "Debug")]
    [Button]
    public void CancelMovement()
    {
        _moveBehavior.ClearCurrentMovement();
    }

}
