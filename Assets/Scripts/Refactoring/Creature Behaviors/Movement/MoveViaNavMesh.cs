using UnityEngine;
using UnityEngine.AI;





public class MoveViaNavMesh : AbstractAiMoveBehavior
{
    //Declarations
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _turnSpeed;
    private NavMeshAgent _navAgent;


    //Monobehaviours
    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
    }




    //Internals
    protected override void ClearOtherMoveUtils()
    {
        _navAgent.isStopped = true;
        _navAgent.ResetPath();
    }

    protected override void SetupOtherDestinationMoveUtils()
    {
        _navAgent.SetDestination(_currentDestination);
    }

    protected override void SetupOtherTargetMoveUtils()
    {
        _navAgent.SetDestination(_currentTarget.position);
    }

    protected override void UpdateOtherTargetBasedMoveUtils()
    {
        _navAgent.SetDestination(_currentTarget.position);
    }

    protected override void UpdateOtherLocationBasedMoveUtils()
    {
        //pass
    }



    //Externals
    public override void ReadCreatureData(CreatureData data)
    {
        _moveSpeed = data.GetBaseMoveSpeed();
        _navAgent.speed = _moveSpeed;
        _turnSpeed = data.GetBaseTurnSpeed();
        _navAgent.angularSpeed = _turnSpeed;
    }
}
