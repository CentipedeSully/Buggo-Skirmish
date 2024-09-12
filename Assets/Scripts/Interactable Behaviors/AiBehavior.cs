using System;
using System.Collections;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
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

    public bool IsReadyForPickup();

    public void TakeDamage(ITargetable aggressor, int damage);

    public bool IsDead();
}


public class AiBehavior : MonoBehaviour, ITargetable
{
    //Declarations
    [Header("References")]
    [SerializeField] private Transform _carryParent;
    [SerializeField] private NavMeshAgent _selfAgent;
    [SerializeField] private Animator _headAnimator;
    [SerializeField] private Animator _bodyAnimator;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private bool _isGravityEnabled = false;
    [SerializeField] private float _gravityResetTime = 3f;

    [Header("Settings")]
    [SerializeField] private InteractableType _interactableType = InteractableType.Minion;
    [SerializeField] private Faction _faction;
    [SerializeField] private float _onDeathThrowForceMin;
    [SerializeField] private float _onDeathThrowForceMax;
    [SerializeField] private float _onDeathThrowTorqueMin;
    [SerializeField] private float _onDeathThrowTorqueMax;
    [SerializeField] private float _interactionRange = .5f;
    [SerializeField] private int _nutrition = 34;
    [SerializeField] private float _interactableDetectionRange = 4;
    [Tooltip("The range in which an idle minion will choose follow the player if the minion has nothing to do")]
    [SerializeField] private float _autoFollowLeaderRange = 1;
    [SerializeField] private Color _interactableRangeGizmoColor = Color.white;
    [SerializeField] private Color _autoFollowLeaderRangeGizmoColor = Color.yellow;
    [SerializeField] private bool _showLeaderDetectionGizmo = false;
    [SerializeField] private bool _showInteractableDetectionGizmo = false;
    private bool _isMoving = false;
    [SerializeField] private LayerMask _interactableLayerMask;
    [SerializeField] private GameObject _leaderObject;
    [SerializeField] private GameObject _nestObject;


    [Header("Behavior State")]
    [SerializeField] private ActorState _currentState = ActorState.Idle;
    [SerializeField] private GameObject _currentTargetObject;
    private ITargetable _targetInterface;
    [SerializeField] private Vector3 _currentTargetGroundPosition;
    [SerializeField] private bool _isMovingToGroundPosition = false;
    private Vector3 _targetPosition;
    [SerializeField] private bool _isTargetInRange = false;
    [SerializeField] private float _distanceFromTarget;
    [SerializeField] private float _closeEnoughDistance = .5f;
    [SerializeField] private bool _isCarryingObject = false;
    private GameObject _carriedObject;
    private bool _isReadyToBePickedUp = false;
    private bool _isPickedUp = false;
    private float _pickupCooldown = 3;
    private bool _isDead = false;

    [Header("Carry Lerp Utils")]
    [SerializeField] private float _pickupDuration = .1f;
    private Vector3 _pickupObjectPosition;
    private float _currentPickupLerpTime = 0;
    private bool _isLerpingPickup = false;

    [Header("Combat Settings")]
    [SerializeField] private int _health = 6;
    [SerializeField] private int _damage = 2;
    [SerializeField] private float _atkCooldown = .5f;
    [SerializeField] private bool _isAtkReady = true;

    [SerializeField] private float _atkCastRadius = 2;
    [SerializeField] private Transform _atkCastOffset;
    [SerializeField] private Color _atkRangeGizmoColor = Color.red;
    [SerializeField] private bool _showAtkRangeGizmo = false;
    [Tooltip("How much tolerance does this minion have when turning to face a target. 0 means no tolerance. 180 means 'I don't need to see you to hit you'")]
    [SerializeField] [Range(0,180)] private float _atkAlignmentTolerance = 10;
    [SerializeField] private float _alignmentRotationSpeed = 90;
    [SerializeField] private bool _showAtkAlignmentRays = false;

    private IEnumerator _atkSequence = null;
    [SerializeField] private float _atkAnimDuration;
    [SerializeField] [Range(0,1)] private float _atkCastRelativeStartTime;
    [SerializeField] private float _atkCastDuration = .2f;
    private float _atkCastStartTime;
    private float _remainingAtkTime;
    [SerializeField] private bool _isAttacking = false;
    private bool _isCastingAttack = false;
    private bool _isAtkCoolingDown = false;
    [SerializeField] private float _damagedAnimResetDelay = .1f;
    [SerializeField] private float _invincTimeAfterHit = .1f;
    [SerializeField] private bool _isInvincible = false;






    //Monobehaviours
    private void Update()
    {
        if (!_isDead)
        {
            ManagePickupLerp();
            WatchSurroundings();
            ValidateTargetingReferences();
            if (!_isAttacking)
                PursueTargetIfTargetAvailable();
            CastAttack();
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawRangeGizmos();
    }


    //Internals
    private void AlignAttack()
    {
        //is our target valid (and we aren't already committed to an attack)
        if (_currentTargetObject != null && !_isAttacking)
        {
            //Calculate the direction towards the target
            Vector3 targetDirection = _currentTargetObject.transform.position - transform.position;

            //Normalize the direction
            Vector3 normalizedDirectionTowardsTarget = targetDirection.normalized;

            //calculate our forwards direction (in world space)
            Vector3 forwardsDirection = transform.TransformVector(Vector3.forward);

            //Draw the alignment vectors used in our calculations, if needed
            if (_showAtkAlignmentRays)
            {
                Debug.DrawRay(transform.position, forwardsDirection * 5, Color.red);
                Debug.DrawRay(transform.position, normalizedDirectionTowardsTarget * 5, Color.yellow);
                
            }

            //calculate the angle between our forwards and the normalizedTargetDirection
            float angleDifference = Vector3.SignedAngle(forwardsDirection, normalizedDirectionTowardsTarget,Vector3.up);
            
            //Log the angular difference, if needed
            //Debug.Log($"angle difference: {angleDifference}");
            
            //is the target within a tolerable alignment
            if (-_atkAlignmentTolerance <= angleDifference && angleDifference <= _atkAlignmentTolerance)
                EnterAttack();

            //we aren't properly aligned to the target
            else
            {
                //calculate the additive rotation. we're rotating on the y axis in the direction of the angular difference's sign
                Vector3 additiveRotation =  Mathf.Sign(angleDifference) * Vector3.up * _alignmentRotationSpeed * Time.deltaTime;

                //Apply the rotation
                transform.eulerAngles += additiveRotation;
            }
            



        }
    }

    private void ReadyAtk()
    {
        _isAtkCoolingDown = false;
        _isAtkReady = true;
    }

    private void CooldownAtk()
    {
        if (!_isAtkCoolingDown)
        {
            _isAtkCoolingDown = true;
            Invoke(nameof(ReadyAtk), _atkCooldown);
        }
    }

    private void CastAttack()
    {
        //if we're in the cast state
        if (_isCastingAttack)
        {
            //Did our target vanish?
            if (_currentTargetObject == null)
            {
                //cancel any timers that're counting the duration of our cast
                CancelInvoke(nameof(EndAttackCast));

                //end the attack cast now
                EndAttackCast();
                return;
            }

            //Cast over the detection area
            Collider[] detections = Physics.OverlapSphere(_atkCastOffset.position, _atkCastRadius, _interactableLayerMask);

            //Attempt to find our target amongst any detections
            foreach (Collider detection in detections)
            {
                //has our target been detected?
                if (detection.gameObject == _currentTargetObject)
                {
                    //Damage the target
                    _targetInterface.TakeDamage(this, _damage);

                    //cancel any timers that're counting the duration of this cast
                    CancelInvoke(nameof(EndAttackCast));

                    //End the cast, the target has been hit
                    EndAttackCast();
                    return;
                }
            }
        }
    }

    private void EndAttackCast()
    {
        _isCastingAttack = false;
    }

    private void EnterAttack()
    {
        //are we NOT already attacking?
        if (_atkSequence == null && _isAtkReady)
        {
            //Set the atk reference
            _atkSequence = ManageAttackSequence();

            //calculate the relative atkCast time
            _atkCastStartTime = _atkAnimDuration * _atkCastRelativeStartTime;

            //calculate the remainder atk time
            _remainingAtkTime = _atkAnimDuration - _atkCastStartTime;

            //start the sequence
            StartCoroutine(_atkSequence);
        }
    }

    private void CancelAttack()
    {
        //are we currently in our attack sequence?
        if (_atkSequence != null)
        {
            //stop the sequence timer
            StopCoroutine(_atkSequence);

            //Clear the reference
            _atkSequence = null;

            //leave the atk state
            _isAttacking = false;

            //stop casting any attacks, if they exist
            CancelInvoke(nameof(CancelAttack));
            EndAttackCast();

            //update the animator that we aren't attacking anymore
            _bodyAnimator.SetBool("isAttacking", false);

            //Cooldown the attack, if it isn't cooling down already
            CooldownAtk();
        }
    }

    private IEnumerator ManageAttackSequence()
    {
        //enter the atk state
        _isAttacking = true;

        //reest the atk ready state
        _isAtkReady = false;

        //enter the proper animation
        _bodyAnimator.SetBool("isAttacking", true);

        //wait for the atkCast time
        yield return new WaitForSeconds(_atkCastStartTime);

        //Cast the atk
        _isCastingAttack = true;
        Invoke(nameof(EndAttackCast), _atkCastDuration);

        //wait for the remainder of the sequence
        yield return new WaitForSeconds(_remainingAtkTime);

        //exit the atk state
        _isAttacking = false;

        //clear our reference
        _atkSequence = null;

        //Exit the attack animation
        _bodyAnimator.SetBool("isAttacking", false);

        //Cooldown the atk
        CooldownAtk();
    }



    private void DrawRangeGizmos()
    {
        if (_showLeaderDetectionGizmo)
        {
            Gizmos.color = _autoFollowLeaderRangeGizmoColor;
            Gizmos.DrawWireSphere(transform.position, _autoFollowLeaderRange);
        }

        if (_showInteractableDetectionGizmo)
        {
            Gizmos.color = _interactableRangeGizmoColor;
            Gizmos.DrawWireSphere(transform.position, _interactableDetectionRange);
        }

        if (_showAtkRangeGizmo)
        {
            Gizmos.color = _atkRangeGizmoColor;
            Gizmos.DrawWireSphere(_atkCastOffset.position, _atkCastRadius);
        }
    }

    private void WatchSurroundings()
    {
        //are we idling and NOT following a move order
        if (_currentState == ActorState.Idle && !_isMovingToGroundPosition)
            DetectAnything();

        //otherwise, are idly moving towards a move order
        else if (_currentState == ActorState.Idle && !_isMovingToGroundPosition)
            DetectHostilesOnly();
    }

    private void DetectAnything()
    {
        //detect anything within range
        Collider[] detectedColliders = Physics.OverlapSphere(transform.position, _interactableDetectionRange, _interactableLayerMask);

        //Detect hostiles first
        GameObject closestInteractableObject = FindClosestDetection(detectedColliders, InteractableType.Minion);

        //if we've found an enemy, attack it
        if (closestInteractableObject != null)
        {
            SetTarget(closestInteractableObject);
            return;
        }


        //Detect pickups
        closestInteractableObject = FindClosestDetection(detectedColliders, InteractableType.Pickup);

        // if anything was found, go after it
        if ( closestInteractableObject != null )
        {
            SetTarget(closestInteractableObject);
            return;
        }


        //Detect the player
        closestInteractableObject = FindClosestDetection(detectedColliders, InteractableType.Player);

        //if the player was detected
        if ( closestInteractableObject != null)
        {
            //attack the player if it isn't on our side
            if (closestInteractableObject.GetComponent<ITargetable>().GetFaction() != _faction)
            {
                SetTarget(closestInteractableObject);
                return;
            }

            //player is friendly. 
            else 
            {
                //calculate the player's distance
                float playerDistance = (closestInteractableObject.transform.position - transform.position).magnitude;

                //Are they within autoFollowRange?
                if (playerDistance <= _autoFollowLeaderRange)
                {
                    SetTarget(closestInteractableObject);

                    //dont forget to add this minion as a follower!
                    closestInteractableObject.GetComponent<PlayerBehavior>().AddFollower(this);

                    return;
                }
            }
        }

    }

    private void DetectHostilesOnly()
    {
        //detect anything within range
        Collider[] detectedColliders = Physics.OverlapSphere(transform.position, _interactableDetectionRange, _interactableLayerMask);

        //Detect hostiles first
        GameObject closestInteractableObject = FindClosestDetection(detectedColliders, InteractableType.Minion);

        //if we've found an enemy, attack it
        if (closestInteractableObject != null)
        {
            SetTarget(closestInteractableObject);
            return;
        }

        //Detect the player last, in case it's hostile
        closestInteractableObject = FindClosestDetection(detectedColliders, InteractableType.Player);

        //if the player was detected
        if (closestInteractableObject != null)
        {
            //attack the player if it isn't on our side
            if (closestInteractableObject.GetComponent<ITargetable>().GetFaction() != _faction)
            {
                SetTarget(closestInteractableObject);
                return;
            }
        }
    }

    private GameObject FindClosestDetection(Collider[] detections,InteractableType preferredType)
    {
        GameObject closestDetection = null;
        float closestDistanceSqrt = 0;

        foreach (Collider detection in detections)
        {
            //attempt to get the detection's behavior
            ITargetable behavior = detection.GetComponent<ITargetable>();

            if (behavior != null)
            {
                //make sure this object matches our preferred interactable type
                if (preferredType == behavior.GetInteractableType())
                {
                    //special pickup cases to watch out for: 
                    if (preferredType == InteractableType.Pickup)
                    {
                        //IGNORE any pickup within range that're already being carried away
                        if (behavior.IsPickedUp())
                            continue;

                        //IGNORE any pickups that aren't ready to be picked up
                        if (!behavior.IsReadyForPickup())
                            continue;
                    }

                    //special case: if we're detecting minions, ignore friendlies
                    if (preferredType == InteractableType.Minion && behavior.GetFaction() == _faction)
                        continue;

                    //Do we have no previous valid detection?
                    if (closestDetection == null)
                    {
                        //this one is the closest by default
                        closestDetection = detection.gameObject;

                        //save the closest distance sqrt (the length of the magnitude^2-- always positive)
                        closestDistanceSqrt = (detection.transform.position - transform.position).sqrMagnitude;
                    }

                    else
                    {
                        //detect this object's distance Sqrt
                        float currentDistanceSqrt = (detection.transform.position - transform.position).sqrMagnitude;

                        //save this detection if it's closer than our previous one
                        if (currentDistanceSqrt < closestDistanceSqrt)
                        {
                            closestDetection = detection.gameObject;
                            closestDistanceSqrt = currentDistanceSqrt;
                        }
                    }
                }
            }
        }

        //return anything we've found (null if nothing was found)
        return closestDetection;
    }

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
            DropObjectIfCarrying();

        //exit the moving anim
        if (_isMoving)
            _bodyAnimator.SetBool("isMoving", false);

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
        try
        {
            //Did a target object vanish while we were following it (ignore if we were only moving to a ground position)
            if (_currentTargetObject == null && _isMovingToGroundPosition == false)
                SetTarget(null);


            else if (_isMovingToGroundPosition == false && _currentTargetObject != null)
            {
                //Did we suddenly lose our behavior reference?
                if (_targetInterface == null || _currentTargetObject == null)
                    SetTarget(null);

                //are we pursuing an object that is already picked up?
                if (_targetInterface.GetInteractableType() == InteractableType.Pickup &&
                    _targetInterface.IsPickedUp())
                    SetTarget(null);

                //are we attempting to pickup an object but have no nest reference for the dropoff?
                if (_targetInterface.GetInteractableType() == InteractableType.Pickup &&
                    _nestObject == null)
                {
                    SetTarget(null);
                    //Debug.LogWarning("Minion attempted to pickup an object without the minion having a reference to its nest");
                }

                //are we carrying a pickup but lost our nest reference
                if (_isCarryingObject && _nestObject == null)
                {
                    DropObjectIfCarrying();
                    //Debug.LogWarning("Nest reference is missing while minion sought to drop off an item");
                }
            }
        }

        catch(NullReferenceException)
        {
            DropObjectIfCarrying();
            SetTarget(null);
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

                //apply the moving animation
                _bodyAnimator.SetBool("isMoving", true);
                _isMoving = true;

            }
        }
    }

    private void EnterInvincAfterHit()
    {
        _isInvincible = true;

        Invoke(nameof(ExitInvinc), _invincTimeAfterHit);
    }

    private void ExitInvinc()
    {
        _isInvincible = false;
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
                _bodyAnimator.SetBool("isMoving", false);
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
                    if (_targetInterface.GetInteractableType() == InteractableType.Pickup)
                    {
                        //is the object available for pickup?
                        if (_targetInterface.IsReadyForPickup())
                        {
                            //pickup the thing
                            PickupObject(_currentTargetObject);

                            //Set the nest as the new target
                            SetTarget(_nestObject);
                        }
                        
                        else
                        {
                            //just forget the target. Nothing more you can do
                            SetTarget(null);
                        }
                    }


                    //else the target is the nest itself
                    else if (_targetInterface.GetInteractableType() == InteractableType.Nest)
                    {
                        //Drop Pickup
                        DropObjectIfCarrying();

                        //You're at the nest. chill
                        SetTarget(null);
                    }
                    break;

                case ActorState.Fight:
                    //is the target dead?
                    if (_targetInterface.IsDead())
                        SetTarget(null);

                    else
                    {
                        //Face the target, and then Attack it!
                        AlignAttack();
                    }

                    break;

            }
        }

        
    }

    private void ManagePickupLerp()
    {
        if ( _isLerpingPickup)
        {
            _currentPickupLerpTime += Time.deltaTime;

            if (_carriedObject != null)
                _carriedObject.transform.localPosition = Vector3.Lerp(_pickupObjectPosition, _carryParent.localPosition, _currentPickupLerpTime / _pickupDuration);

            if (_currentPickupLerpTime >= _pickupDuration || _carriedObject == null)
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
        else if (_isMovingToGroundPosition != false)
            UpdateMovingToGroundPositionState();
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

    private void DropObjectIfCarrying()
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

    private void UpdateMovingToGroundPositionState()
    {
        //have we arrived at our target location?
        if ( _isMovingToGroundPosition)
        {
            //calculate the distance from our target
            _distanceFromTarget = Mathf.Abs((_currentTargetGroundPosition - transform.position).magnitude);

            //are we close enough to our target position?
            if (_distanceFromTarget <= _closeEnoughDistance)
            {
                //update our move states
                _isMovingToGroundPosition = false;
                _isMoving = false;

                //exit the moving anim
                _bodyAnimator.SetBool("isMoving", false);
            }
            

        }
    }

    private void Die()
    {
        _isDead = true;
        _interactableType = InteractableType.Pickup;
        _faction = Faction.Neutral;

        //Set animator state to isDead
        _bodyAnimator.SetBool("isDead", true);

        //disable the navAgent
        _selfAgent.enabled = false;

        //throw the body around a bit ^_^
        TemporarilyEnableGravity();
        float xRandom = UnityEngine.Random.Range(-1, 1);
        float yRandom = UnityEngine.Random.Range(-1, 1);
        float zRandom = UnityEngine.Random.Range(-1, 1);
        
        //be sure to remove the rotational constraints for *authentic ragdoll flavor
        _rb.constraints = RigidbodyConstraints.None;
        _rb.AddForce(Vector3.up * UnityEngine.Random.Range(_onDeathThrowForceMin, _onDeathThrowForceMax), ForceMode.Impulse);
        _rb.AddTorque(new Vector3(xRandom, yRandom, zRandom) * UnityEngine.Random.Range(_onDeathThrowTorqueMin, _onDeathThrowTorqueMax), ForceMode.Impulse);


        //make it available for pickup after a delay
        Invoke(nameof(ReadyPickup), _pickupCooldown);

        //remove ourself from the leader's followers
        if (_leaderObject != null)
            _leaderObject.GetComponent<PlayerBehavior>().RemoveFollower(this);
    }

    private void DisableGravity()
    {
        if (_isGravityEnabled)
        {
            _rb.useGravity = false;
            _isGravityEnabled = false;

            CancelInvoke(nameof(DisableGravity));
        }
    }

    private void TemporarilyEnableGravity()
    {
        //is gravity off?
        if (!_isGravityEnabled)
        {
            _rb.useGravity = true;
            _isGravityEnabled = true;

            //start counting down before gravity is stopped
            Invoke(nameof(DisableGravity), _gravityResetTime);
        }
    }

    private void ReadyPickup()
    {
        _isReadyToBePickedUp = true;
    }


    //Externals
    public int Nutrition()
    {
        return _nutrition;
    }

    public void SetPursuitTarget(ITargetable target)
    {
        //is the target NOT OURSELF
        if (target.GetBehaviorID() != GetInstanceID())
            SetTarget(target.GetGameObject());
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

    public void MoveToPosition(Vector3 position)
    {
        //clear the current targeting data
        if (_currentTargetObject != null)
            SetTarget(null);

        //set the moving to ground position state
        _isMovingToGroundPosition = true;
        _currentTargetGroundPosition = new Vector3(position.x, transform.position.y, position.z);

        //set the move animation
        _bodyAnimator.SetBool("isMoving", true);

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
        return _isPickedUp;
    }

    public bool IsReadyForPickup()
    {
        return _isReadyToBePickedUp;
    }

    public void TakeDamage(ITargetable aggressor, int damage)
    {
        if (_currentState != ActorState.Fight)
        {
            //Drop everything
            DropObjectIfCarrying();

            //target our attacker. We'll enter the fight state if our aggressor isn't friendly
            SetTarget(aggressor.GetGameObject());
        }

        //interrupt any attacks
        if (_isAttacking)
            CancelAttack();

        //take the damage
        _health -= damage;

        //animate the hit
        _bodyAnimator.SetBool("isDamaged", true);

        //reset the damaged anim (staying away from animation triggers ^_^)
        Invoke(nameof(ResetDamagedAnimation), _damagedAnimResetDelay);

        //Die if we ded
        if (_health <= 0)
        {
            Die();
            return;
        }
            

        else
        {
            //enter invinc to avoid too-frequent frame-after-frame hits
            EnterInvincAfterHit();
        }

    }

    private void ResetDamagedAnimation()
    {
        _bodyAnimator.SetBool("isDamaged", false);
    }

    public bool IsDead()
    {
        return _isDead;
    }

    public void SetPickupState(bool newState)
    {
        if (_interactableType == InteractableType.Pickup)
        {
            _isPickedUp = newState;

            //make sure gravity is disabled when being carried
            if (_isPickedUp)
                DisableGravity();

            //make sure to enable gravity when being dropped
            else if (!_isPickedUp)
            {
                TemporarilyEnableGravity();

                //cooldown the pickup, so it doen't get juggled each frame by ai
                _isReadyToBePickedUp = false;
                Invoke(nameof(ReadyPickup), _pickupCooldown);
            }
        }
    }

    //Debugging





}
