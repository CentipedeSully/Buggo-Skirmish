using UnityEngine;
using UnityEngine.AI;





public class GruntBehavior : MonoBehaviour
{
    //Declarations
    [Header("References")]
    [SerializeField] private Transform _carryParent;
    [SerializeField] private NavMeshAgent _selfAgent;

    [Header("Settings")]
    [SerializeField] private float _interactionRange = .5f;
    private bool _isApproachingTarget = false;
    [SerializeField] private LayerMask _pickupLayerMask;
    [SerializeField] private LayerMask _actorLayerMask;
    [SerializeField] private GameObject _leaderObject;
    [SerializeField] private GameObject _nestObject;


    [Header("Behavior State")]
    [SerializeField] private ActorState _currentState = ActorState.Idle;
    [SerializeField] private GameObject _currentTarget;
    [SerializeField] private GameObject _carriedObject;



    //Monobehaviors





    //Internals
    private void SetTarget(GameObject target)
    {
        if (_currentTarget != target)
        {
            //Replace the current Target
            _currentTarget = target;

            //reset movement utils
            _isApproachingTarget = false;
            _selfAgent.isStopped = true;
        }
    }

    private float CalculateDistanceToTarget()
    {
        float distance = Vector3.Distance(_currentTarget.transform.position, transform.position);

        Debug.Log($"Calculated distance: {distance}");
        return distance;
    }

    private void ApproachTheCurrentTarget()
    {
        //if we're beyond range of this target (AND we aren't already pathing to this target)
        if (CalculateDistanceToTarget() > _interactionRange && _isApproachingTarget == false)
        {
            //update movement state
            _isApproachingTarget=true;

            //moveTowards the currentTarget
            _selfAgent.SetDestination(_currentTarget.transform.position);
        }
    }

    private void InteractWithCurrentTarget()
    {
        //stop approaching if we're close enough
        if (CalculateDistanceToTarget() <= _interactionRange)
        {
            if (_isApproachingTarget)
            {
                _isApproachingTarget = false;
                _selfAgent.isStopped = true;
            }
        }

        //Determine our interaction
        switch (_currentState) 
        {
            case ActorState.Idle:
                //chill
                break;

            case ActorState.Follow:
                //chill
                break;

            case ActorState.Collect:
                //if the target is a pickup?
                //pickup the thing
                //...

                //Set the nest as the target
                //...

                //goTo the nest
                //...

                //else the target is the nest itself
                //destroy the pickup
                //...

                //reset pickup utils
                //...

                //enter the Idle
                //...
                break;

            case ActorState.Fight:
                //enable the fight logic
                break;
        
        }

    }

    
    private void FollowLeader()
    {
        //Stop following leader if there isn't one
        if ( _leaderObject == null)
        {
            _currentState = ActorState.Idle;
            return;
        }

        //Target our leader if it isn't yet targeted
        else if (_leaderObject != _currentTarget)
        {
            //Target the leader
            SetTarget(_leaderObject);

            //follow the leader
            ApproachTheCurrentTarget();
        }
    }




    //Externals





    //Debugging







}
