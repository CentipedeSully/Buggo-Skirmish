using UnityEngine;
using UnityEngine.AI;



public enum ActorState
{
    Idle,
    Follow,
    Collect,
    Fight
}

public enum InteractableType
{
    Unset,
    Player,
    Minion,
    Pickup,
    Nest
}

public enum Faction
{
    NotApplicable,
    Neutral,
    Ally,
    Enemy
}

public interface ITargetable
{
    public int GetBehaviorID();

    public InteractableType GetInteractableType();

    public Faction GetFaction();

    public GameObject GetGameObject();

    public int Nutrition();

    public void TriggerGravity();
}


public class AiBehavior : MonoBehaviour, ITargetable
{
    //Declarations
    [Header("References")]
    [SerializeField] private Transform _carryParent;
    [SerializeField] private NavMeshAgent _selfAgent;
    [SerializeField] private Animator _headAnimator;

    [Header("Settings")]
    [SerializeField] private InteractableType _interactableType = InteractableType.Minion;
    [SerializeField] private Faction _faction;
    [SerializeField] private float _interactionRange = .5f;
    [SerializeField] private int _nutrition = 34;
    private bool _isMoving = false;
    [SerializeField] private LayerMask _pickupLayerMask;
    [SerializeField] private LayerMask _actorLayerMask;
    [SerializeField] private GameObject _leaderObject;
    [SerializeField] private GameObject _nestObject;


    [Header("Behavior State")]
    [SerializeField] private ActorState _currentState = ActorState.Idle;
    [SerializeField] private GameObject _currentTarget;
    [SerializeField] private Vector3 _targetPosition;
    [SerializeField] private bool _isTargetInRange = false;
    [SerializeField] private float _distanceFromTarget;
    [SerializeField] private GameObject _carriedObject;
    private bool _isHeadTilted = false;

    [Header("Carry Lerp Utils")]
    [SerializeField] private float _pickupDuration = .1f;
    private Vector3 _pickupObjectPosition;
    private float _currentPickupLerpTime = 0;
    private bool _isLerpingPickup = false;




    //Monobehaviours
    private void Update()
    {
        ManagePickupLerp();
        PursueTargetIfTargetAvailable();
    }




    //Internals
    private void SetTarget(GameObject target)
    {
        //Replace the current Target
        _currentTarget = target;

        //reset movement utils
        _isMoving = false;
        _selfAgent.isStopped = true;
    }

    private void CalculateDistanceFromCurrentTarget()
    {
        _distanceFromTarget = Vector3.Distance(_currentTarget.transform.position, transform.position);

        if (_distanceFromTarget <= _interactionRange)
            _isTargetInRange = true;

        else 
            _isTargetInRange = false;
    }

    private void ApproachTheCurrentTarget()
    {
        //is it far enough to justify a move order?
        if (_isTargetInRange == false)
        {
            //are we NOT already moving towards this position
            if (!_isMoving || _targetPosition != _currentTarget.transform.position)
            {
                //save the move position
                _targetPosition = _currentTarget.transform.position;

                //move towards the position
                _selfAgent.SetDestination(_targetPosition);
                _selfAgent.isStopped = false;

                _isMoving=true;

            }
        }
    }

    private void InteractWithCurrentTarget()
    {
        if (_isTargetInRange)
        {
            //are we still moving towards the target?
            if (_isMoving)
            {
                //stopMoving towards the target
                _selfAgent.isStopped = true;

                _isMoving = false;
            }
            

            //Determine our interaction
            switch (_currentState)
            {
                case ActorState.Idle:
                    //chill
                    _currentTarget = null;
                    break;

                case ActorState.Follow:
                    //chill
                    break;

                case ActorState.Collect:
                    //get the behavior of the pickup
                    ITargetable behavior = _currentTarget.GetComponent<ITargetable>();

                    //if the target is a pickup?
                    if (behavior.GetInteractableType() == InteractableType.Pickup)
                    {
                        //pickup the thing
                        PickupObject(_currentTarget);

                        //Set the nest as the target
                        SetTarget(_nestObject);
                    }


                    //else the target is the nest itself
                    else if (behavior.GetInteractableType() == InteractableType.Nest)
                    {
                        //Drop Pickup
                        DropObject();

                        //enter the Idle
                        _currentState = ActorState.Idle;
                    }
                    break;

                case ActorState.Fight:
                    //enable the fight logic
                    break;

            }
        }

        
    }

    private void ManagePickupLerp()
    {
        if ( _isLerpingPickup)
        {
            _currentPickupLerpTime += Time.deltaTime;
            _carriedObject.transform.localPosition = Vector3.Lerp(_pickupObjectPosition, _carryParent.localPosition, _currentPickupLerpTime / _pickupDuration);

            if (_currentPickupLerpTime >= _pickupDuration)
            {
                _isLerpingPickup = false;
                _currentPickupLerpTime = 0;
            }
            
        }
    }

    private void PursueTargetIfTargetAvailable()
    {
        if (_currentTarget != null)
        {
            CalculateDistanceFromCurrentTarget();
            ApproachTheCurrentTarget();
            InteractWithCurrentTarget();
        }
        else
        {
            _isMoving = false;
            _currentState = ActorState.Idle;
            _isTargetInRange = false;
        }
    }


    private void PickupObject(GameObject targetObject)
    {
        if (_carriedObject == null)
        {
            _carriedObject = targetObject;
            _carriedObject.transform.SetParent(_carryParent);

            //show pickup animation
            _headAnimator.SetBool("isCarrying", true);

            //setup lerp utils
            _pickupObjectPosition = _carriedObject.transform.localPosition;
            _currentPickupLerpTime = 0;

            //start lerping
            _isLerpingPickup = true;
        }
    }

    private void DropObject()
    {
        if (_carriedObject != null)
        {
            _carriedObject.transform.SetParent(null);

            //Toggle gravity for the funny "toss it away" effect ^_^
            _carriedObject.GetComponent<ITargetable>().TriggerGravity();

            _carriedObject = null;

            //show drop animation
            _headAnimator.SetBool("isCarrying", false);


        }
    }


    //Externals
    public int Nutrition()
    {
        return _nutrition;
    }

    public void SetPursuitTarget(ITargetable target)
    {
        //is the target not null and NOT OURSELF
        if (target != null && target.GetBehaviorID() != GetInstanceID())
        {
            //Identify the target
            switch (target.GetInteractableType())
            {
                case InteractableType.Minion:

                    //is this minion against us?
                    if (target.GetFaction() != _faction)
                    {
                        //set the minion as our target
                        SetTarget(target.GetGameObject());

                        //enter attack mode
                        _currentState = ActorState.Fight;
                    }
                    break;


                case InteractableType.Player:

                    //set the player as our pursuit target
                    SetTarget(target.GetGameObject());

                    //enter attack mode if the player isn't on our side
                    if (target.GetFaction() != _faction)
                        _currentState = ActorState.Fight;

                    //otherwise just follow the player
                    else
                        _currentState = ActorState.Follow;

                    break;


                case InteractableType.Pickup:
                    SetTarget(target.GetGameObject());
                    _currentState = ActorState.Collect;
                    break;


                case InteractableType.Nest:
                    SetTarget(target.GetGameObject());
                    _currentState = ActorState.Collect;
                    break;


                case InteractableType.Unset:
                    Debug.LogWarning($"Unset Interactable type detected ({target.GetGameObject()}). Minion ignoring object.");
                    break;
            }

        }

        

        
    }

    public int GetBehaviorID()
    {
        return GetInstanceID();
    }

    public InteractableType GetInteractableType()
    {
        return _interactableType;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public Faction GetFaction()
    {
        return _faction;
    }

    public void TriggerGravity()
    {
        GetComponent<Rigidbody>().useGravity = true;
    }


    //Debugging





}
