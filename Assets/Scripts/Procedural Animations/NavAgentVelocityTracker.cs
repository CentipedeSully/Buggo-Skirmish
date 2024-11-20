using UnityEngine;
using UnityEngine.AI;

public class NavAgentVelocityTracker : MonoBehaviour
{
    //declarations
    private NavMeshAgent _agent;
    private CommunicateToAnimators _animatorCommunicator;




    //monobehaviours
    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animatorCommunicator = GetComponent<CommunicateToAnimators>();
    }

    private void Update()
    {
        DisplaceFeetByNavAgentVelocity();
    }


    //internals
    private void DisplaceFeetByNavAgentVelocity()
    {
        if (_agent != null && _animatorCommunicator != null)
            _animatorCommunicator.MoveFeetViaDisplacement(_agent.velocity);
    }

    //externals





}
