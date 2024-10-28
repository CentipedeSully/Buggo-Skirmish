using UnityEngine;

public class PlayerCreatureController : AbstractCreatureBehavior
{
    //Declarations
    private IPlayerMoveBehavior _moveBehavior;
    private IAttackBehaviour _attackBehavior;
    private ICarryBehavior _carryBehavior;



    //Monobehaviours





    //Internals
    protected override void InitializeCreatureReferences()
    {
        base.InitializeCreatureReferences();
        _moveBehavior = GetComponent<IPlayerMoveBehavior>();
        _attackBehavior = GetComponent<IAttackBehaviour>();
        _carryBehavior = GetComponent<ICarryBehavior>();

    }



    //Externals
}
