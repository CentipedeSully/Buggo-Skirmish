using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;



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

    public void SetPickupState(bool state);

    public bool IsPickedUp();
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
    [SerializeField] private GameObject _currentTargetObject;
    private ITargetable _targetInterface;
    private Vector3 _currentTargetGroundPosition;
    private bool _isMovingToGroundPosition = false;
    private Vector3 _targetPosition;
    [SerializeField] private bool _isTargetInRange = false;
    [SerializeField] private float _distanceFromTarget;
    [SerializeField] private bool _isCarryingObject = false;
    private GameObject _carriedObject;
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
        ValidateTargetingReferences();
        PursueTargetIfTargetAvailable();
    }




    //Internals
    private void SetTarget(GameObject target)
    {
        if (_isMovingToGroundPosition)
            _isMovingToGroundPosition = false;

        //Replace the current Target
        _currentTargetObject = target;

        //cache the target's behavior
        if (_currentTargetObject != null)
            _targetInterface = _currentTargetObject.GetComponent<ITargetable>();
        else _targetInterface = null;

        //Determine your state based on your new target
        DetermineStateBasedOnTarget();

        //drop what you're doing if we're being re-tasked
        if (_isCarryingObject && _currentState != ActorState.Collect)
            DropObject();

        //reset movement utils
        _isMoving = false;
        _selfAgent.isStopped = true;
    }

    private void DetermineStateBasedOnTarget()
    {
        //go idle if the target got cleared
        if (_currentTargetObject == null)
        {
            _currentState = ActorState.Idle;
            return;
        }

        //are we fetching a pickup or carrying a pickup to the nest?
        if (_targetInterface.GetInteractableType() == InteractableType.Pickup ||
            (_targetInterface.GetInteractableType() == InteractableType.Nest && _isCarryingObject) )
        {
            _currentState = ActorState.Collect;
        }

        //are we targeting the player?
        else if (_targetInterface.GetInteractableType() == InteractableType.Player)
        {
            //follow the leader if we're on the same team
            if (_targetInterface.GetFaction() == _faction)
                _currentState = ActorState.Follow;

            //else Eff that guy!
            else 
                _currentState = ActorState.Fight;
        }
            
        //are we targeting an enemy minion?
        else if (_targetInterface.GetInteractableType() == InteractableType.Minion &&
            _targetInterface.GetFaction() != _faction )
        {
            _currentState = ActorState.Fight;
        }
            
    }

    private void ValidateTargetingReferences()
    {
        //did the target vanitsh?
        if (_currentTargetObject == null)
            SetTarget(null);


        else
        {
            //are we pursuing an object that is already picked up?
            if (_targetInterface.GetInteractableType() == InteractableType.Pickup && 
                _targetInterface.IsPickedUp())
                SetTarget(null);

            //are we attempting to pickup an object but have no nest reference for the dropoff?
            if (_targetInterface.GetInteractableType() == InteractableType.Pickup &&
                _nestObject == null)
            {
                SetTarget(null);
                Debug.LogWarning("Minion attempted to pickup an object without the minion having a reference to its nest");
            }

            //are we carrying a pickup but lost our nest reference
            if (_isCarryingObject && _nestObject == null)
            {
                DropObject();
                Debug.LogWarning("Nest reference is missing while minion sought to drop off an item");
            }
        }
    }

    private void CalculateDistanceFromCurrentTarget()
    {
        _distanceFromTarget = Vector3.Distance(_currentTargetObject.transform.position, transform.position);

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
            if (!_isMoving || _targetPosition != _currentTargetObject.transform.position)
            {
                //save the move position. Do this to to avoid repathing to an object that isn't changing its position
                _targetPosition = _currentTargetObject.transform.position;

                //move towards the position
                _selfAgent.SetDestination(_targetPosition);
                _selfAgent.isStopped = false;

                _isMoving = true;

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
                    _currentTargetObject = null;
                    break;

                case ActorState.Follow:
                    //chill
                    break;

                case ActorState.Collect:
                    //if the target is a pickup?
                    if (_targetInterface.GetInteractableType() == InteractableType.Pickup)
                    {
                        //pickup the thing
                        PickupObject(_currentTargetObject);

                        //Set the nest as the target
                        SetTarget(_nestObject);
                    }


                    //else the target is the nest itself
                    else if (_targetInterface.GetInteractableType() == InteractableType.Nest)
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
        if (_currentTargetObject != null)
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

            //make sure the object knows its picked up
            _carriedObject.GetComponent<ITargetable>().SetPickupState(true);

            //show pickup animation
            _headAnimator.SetBool("isCarrying", true);

            //setup lerp utils
            _pickupObjectPosition = _carriedObject.transform.localPosition;
            _currentPickupLerpTime = 0;

            //start lerping
            _isLerpingPickup = true;

            _isCarryingObject = true;
        }
    }

    private void DropObject()
    {
        if (_carriedObject != null)
        {
            //tell the object its been dropped
            _carriedObject.GetComponent<ITargetable>().SetPickupState(false);

            //unparent
            _carriedObject.transform.SetParent(null);

            //clear our reference to the object
            _carriedObject = null;

            //show drop animation
            _headAnimator.SetBool("isCarrying", false);

            _isCarryingObject = false;


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
            SetTarget(target.GetGameObject());
        else
            SetTarget(null);
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

    public void SetPickupState(bool state)
    {
        GetComponent<Rigidbody>().useGravity = true;
    }

    public void MoveToPosition(Vector3 position)
    {
        //clear the current targeting data
        if (_currentTargetObject != null)
            SetTarget(null);
            
        //Simply move to the destination
        _isMoving = true;
        _selfAgent.isStopped = false;
        _selfAgent.SetDestination(position);
    }

    public void SetNest(GameObject nest)
    {
        _nestObject = nest;
    }

    public bool IsPickedUp()
    {
        return false;
    }

    //Debugging





}
