using UnityEngine;
using UnityEngine.AI;





public class MoveViaNavMesh : AbstractAiMoveBehavior
{
    //Declarations
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _turnSpeed;
    private NavMeshAgent _navAgent;
    private CommunicateToAnimators _animatorCommunicator;


    //Monobehaviours
    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _animatorCommunicator =GetComponent<CommunicateToAnimators>();
    }

    private void Update()
    {
        CommunicateMovementToAnimator();
    }


    //Internals
    protected override void ClearOtherMoveUtils()
    {
        _navAgent.isStopped = true;
        _navAgent.ResetPath();

        EndAnimatorMovement();
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

    private void CommunicateMovementToAnimator()
    {
        if (_animatorCommunicator != null)
        {
            //move feets
            if (_navAgent.velocity.magnitude > 0)
                _animatorCommunicator.MoveFeetViaDisplacement(_navAgent.velocity);

            //turn head
            _animatorCommunicator.TurnHead(_navAgent.velocity);
        }
        
    }
    private void EndAnimatorMovement()
    {
        if (_animatorCommunicator != null)
            _animatorCommunicator.StopTurningHead();
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
