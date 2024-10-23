using UnityEngine;
using Sirenix;
using Sirenix.OdinInspector;
using System.Collections.Generic;





public class CoreMinionAi : AbstractCreatureBehavior
{
    //Declarations
    private IAiMoveBehavior _moveBehavior;



    //Monobehaviours




    //Internals
    protected override void  InitializeReferences()
    {
        base.InitializeReferences();
        _moveBehavior = GetComponent<IAiMoveBehavior>();
    }



    //Externals
    [TabGroup("Creature Behavior", "Debug")]
    [Button]
    public void MoveToPosition(Vector3 position)
    {
        _moveBehavior.MoveToLocation(position);
    }

    [TabGroup("Creature Behavior", "Debug")]
    [Button]
    public void ApproachTarget(Transform target)
    {
        _moveBehavior.ApproachTarget(target);
    }

    [TabGroup("Creature Behavior", "Debug")]
    [Button]
    public void CancelMovement()
    {
        _moveBehavior.ClearCurrentMovement();
    }
}
