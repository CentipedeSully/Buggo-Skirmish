using UnityEngine;
using UnityEngine.AI;





public class MoveViaNavMesh : AbstractAiMoveBehavior
{
    //Declarations
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _turnSpeed;
    private NavMeshAgent _navAgent;
    private CommunicateDisplacementToFeet _feetDisplacer;


    //Monobehaviours
    private void Awake()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _feetDisplacer =GetComponent<CommunicateDisplacementToFeet>();
    }

    private void Update()
    {
        CommunicateWithFeetDisplacer();
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

    private void CommunicateWithFeetDisplacer()
    {
        if (_navAgent.velocity.magnitude > 0 && _feetDisplacer != null )
            _feetDisplacer.MoveFeetViaDisplacement(_navAgent.velocity);
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
