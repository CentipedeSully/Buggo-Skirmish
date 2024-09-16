using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Windows;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.Animations;



public class PlayerBehavior : MonoBehaviour, ITargetable
{
    //Declarations
    [SerializeField] private Camera _mainCam;
    [SerializeField] private Transform _cameraParent;
    [SerializeField] private PlayerInput _playerInputReference;
    [SerializeField] private PlayerManipulator _playerManipulator;
    [SerializeField] private Transform _bodyModelTransform;
    [SerializeField] private Rigidbody _rb;
    private GameManager _gameManager;

    private InputAction _moveInputAction;
    private InputAction _callUnitsInputAction;
    private InputAction _mouseMovementInputAction;
    private InputAction _mouseLclickInputAction;
    private InputAction _mouseRclickInputAction;
    private InputAction _pauseInputAction;


    private Vector2 _moveInput;
    private float _cameraInput;
    private bool _callUnitsInput;

    [SerializeField] private float _moveSpeed = 3;
    [SerializeField] private float _cameraRotationSpeed = 20f;
    [SerializeField] private float _signalRadius = 10f;
    [SerializeField] private Color _signalRadiusGizmoColor = Color.green;
    [SerializeField] private float _actionCooldownDuration = .1f;
    private bool _isUnitCallReady = true;
    
    [SerializeField] private float _turnSpeed = 180;
    [SerializeField] private float _lookAngleTolerance = 10;

    private Vector3 _cameraRelativeMoveDirection;
    private Vector3 _worldMoveVector;
    [SerializeField] private Color _moveDirectionGizmoColor = Color.cyan;
    [SerializeField] private float _gizmoLineLength = 5;

    [SerializeField] private List<AiBehavior> _activeFollowers;
    [SerializeField] private int _health;
    private bool _isDead;

    [SerializeField] private Animator _bodyAnimator;
    [SerializeField] private float _damagedAnimResetDelay = .1f;
    [SerializeField] private bool _isInvincible = false;
    [SerializeField] private float _invincTimeAfterHit = .1f;
    [SerializeField] private float _deathThrowForce = 700;
    [SerializeField] private float _deathThrowTorqueMax = 600;
    [SerializeField] private float _deathThrowTorqueMin = 300;



    //Monobehaviours
    private void Start()
    {
        //make the player ignore the idel and move anims (they break, currently when the player turns)
        _bodyAnimator.SetBool("isPlayer", true);

        //Collect action references
        _moveInputAction = _playerInputReference.actions.FindAction("PlayerMovement");
        _callUnitsInputAction = _playerInputReference.actions.FindAction("Call Units");
        _mouseMovementInputAction = _playerInputReference.actions.FindAction("CameraMovement");
        _mouseLclickInputAction = _playerInputReference.actions.FindAction("LClick");
        _mouseRclickInputAction = _playerInputReference.actions.FindAction("RClick");
        _pauseInputAction = _playerInputReference.actions.FindAction("Pause Menu");
    }

    private void Update()
    {
        ReadInput();
        TurnCamera();

        if (!_isDead)
        {
            MovePlayer();
            CallNearbyUnits();
        }
    }


    private void OnDrawGizmosSelected()
    {
        DrawSignalRadiusGizmo();
        DrawMoveDirectionGizmo();
    }


    //Internals
    private void ReadInput()
    {
        //Movement
        if (_moveInputAction.IsPressed())
            _moveInput = _moveInputAction.ReadValue<Vector2>();
        else
            _moveInput = Vector2.zero;

        //Call units input
        if (_callUnitsInputAction.WasPressedThisFrame())
            _callUnitsInput = true;
        else 
            _callUnitsInput = false;

        //Mouse Movement
        if (_mouseMovementInputAction.IsPressed())
            _cameraInput = _mouseMovementInputAction.ReadValue<float>();
        else
            _cameraInput = 0;

        //Lclick
        if (_mouseLclickInputAction.WasPressedThisFrame())
            if (!_isDead)
                GiveCommandViaManipulatorContext();

        //Pause
        if (_pauseInputAction.WasPressedThisFrame())
            _gameManager.Pause();
            

        
    }

    private void MovePlayer()
    {
        if (_moveInput != Vector2.zero)
        {

            //create the raw move vector
            Vector3 moveVector = new Vector3(_moveInput.x, 0, _moveInput.y);

            //relate the raw vector into the camera's local space
            _cameraRelativeMoveDirection = _cameraParent.transform.TransformDirection(moveVector);

            //calculate the properly-displaced move vector
            _worldMoveVector = transform.position + _cameraRelativeMoveDirection;

            //calculate our forwards direction
            Vector3 bodyForwardsVector = _bodyModelTransform.TransformVector(Vector3.forward);

            //Draw the forwards direction for debug purposes (if necessary)
            Debug.DrawRay(_bodyModelTransform.position, bodyForwardsVector * 5);

            //move the player
            transform.Translate(_moveSpeed * Time.deltaTime * _cameraRelativeMoveDirection);

            //calculate the angle from the body forwards to themove vector
            float signedAngle  = Vector3.SignedAngle(bodyForwardsVector, _cameraRelativeMoveDirection, Vector3.up);

            //Log the math, for clarity
            Debug.Log($"Player Angle: {signedAngle}");

            //are we currently outside the specified angular tolerance
            if ( signedAngle < -_lookAngleTolerance || signedAngle > _lookAngleTolerance)
            {
                //Calculate a rotation additive that's in the opposite direction of our signed angular difference
                Vector3 additiveRotation = Vector3.up * Time.deltaTime * _turnSpeed * Mathf.Sign(signedAngle);

                //apply the rotation to the object
                _bodyModelTransform.eulerAngles += additiveRotation;
            }
            
        }
        
    }

    private void DrawMoveDirectionGizmo()
    {
        Gizmos.color = _moveDirectionGizmoColor;
        Gizmos.DrawLine(transform.position,  transform.position +  _gizmoLineLength * _cameraRelativeMoveDirection);
    }

    private void TurnCamera()
    {
        if (_cameraInput != 0)
        {
            //get the old euler angle
            Vector3 oldRotation = _cameraParent.transform.rotation.eulerAngles;

            //create the additive
            Vector3 additiveRotation = new Vector3(0, _cameraInput * _cameraRotationSpeed * Time.deltaTime, 0);

            //calculate the sum rotation
            Vector3 sumRotation = oldRotation - additiveRotation;

            //Apply the new rotation
            _cameraParent.transform.eulerAngles = sumRotation;
        }

        _cameraParent.transform.position = transform.position;
    }

    private void ReadyUnitCall()
    {
        _isUnitCallReady = true;
    }

    private void CallNearbyUnits()
    {
        if (_isUnitCallReady && _callUnitsInput == true)
        {
            //updat the action state
            _isUnitCallReady = false;

            Collider[] detections = Physics.OverlapSphere(transform.position, _signalRadius);

            foreach (Collider collider in detections)
            {
                ITargetable behavior = collider.gameObject.GetComponent<ITargetable>();

                if (behavior != null)
                {
                    //if this object is an ally minion
                    if (behavior.GetFaction() == Faction.Ally && behavior.GetInteractableType() == InteractableType.Minion)
                    {
                        //create a new non-interface variable
                        AiBehavior aiBehavior = (AiBehavior)behavior;

                        //follow us
                        aiBehavior.SetPursuitTarget(this);

                        //add this follower as a follower
                        AddMinionToFollowerList(aiBehavior);
                    }
                }
            }

            //begin the action's cooldown
            Invoke(nameof(ReadyUnitCall),_actionCooldownDuration);
        }
        
    }

    private void DrawSignalRadiusGizmo()
    {
        Gizmos.color = _signalRadiusGizmoColor;
        Gizmos.DrawWireSphere(transform.position,_signalRadius);
    }

    private void GiveCommandViaManipulatorContext()
    {
        //capture mouse-position
        _playerManipulator.CastMouseRay();

        if (!_playerManipulator.IsAnythingDetected())
            return;

        //did we detect an object that isn't the ground?
        if (_playerManipulator.IsNonGroundObjectDetected())
        {

            ITargetable target = _playerManipulator.GetDetectedObject().GetComponent<ITargetable>();

            //Ally? (and not ourself!)
            if (target.GetFaction() == Faction.Ally && target.GetBehaviorID() != GetInstanceID())
            {
                //get the ally's behavior
                AiBehavior minion = target.GetGameObject().GetComponent<AiBehavior>();

                //Come to me!
                minion.SetPursuitTarget(this);

                //add this follower to the list
                AddMinionToFollowerList(minion);
            }

            //It isn't an ally...
            else
            {
                //do we have any minions to command?
                if (_activeFollowers.Count > 0)
                {
                    //get the next minion in the list
                    AiBehavior minion = _activeFollowers[0];

                    //Get it!
                    minion.SetPursuitTarget(target);

                    //Let the minion work. Remove it from the followers list
                    _activeFollowers.RemoveAt(0);
                }
            }
        }

        //did we detected the ground
        else
        {
            //do we have any minions to command?
            if (_activeFollowers.Count > 0)
            {
                //get the next minion in the list
                AiBehavior minion = _activeFollowers[0];

                //Go there!
                minion.MoveToPosition(_playerManipulator.GetGroundDetectionPoint());

                //Let the minion go. Remove it from the followers list
                _activeFollowers.RemoveAt(0);
            }
        }
    }

    private void AddMinionToFollowerList(AiBehavior minionBehavior)
    {
        if (!_activeFollowers.Contains(minionBehavior))
            _activeFollowers.Add(minionBehavior);
    }

    private void ResetDamagedAnimation()
    {
        _bodyAnimator.SetBool("isDamaged", false);
    }

    private void EnterInvincAfterHit()
    {
        _isInvincible = true;

        Invoke(nameof(ExitInvinc), _invincTimeAfterHit);
    }

    private void ExitInvinc() { _isInvincible = false; }

    private void Die()
    {
        _isDead = true;

        //disable the navmesh agent
        GetComponent<NavMeshAgent>().enabled = false;

        //remove the player's rotation contraints
        _rb.constraints = RigidbodyConstraints.None;

        //enable gravity
        _rb.useGravity = true;

        //Unparent the camera
        _cameraParent.SetParent(null);

        //generate the throw forces
        float xRandom = UnityEngine.Random.Range(-1, 1);
        float yRandom = UnityEngine.Random.Range(-1, 1);
        float zRandom = UnityEngine.Random.Range(-1, 1);
        _rb.AddForce(Vector3.up * _deathThrowForce, ForceMode.Impulse);
        _rb.AddTorque(new Vector3(xRandom, yRandom, zRandom) * UnityEngine.Random.Range(_deathThrowTorqueMin, _deathThrowTorqueMax), ForceMode.Impulse);
    }




    //Externals
    public int GetBehaviorID()
    {
        return GetInstanceID();
    }

    public InteractableType GetInteractableType()
    {
        return InteractableType.Player;
    }

    public Faction GetFaction()
    {
        return Faction.Ally;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public int Nutrition()
    {
        return 0;
    }

    public void SetPickupState(bool state)
    {
        //ignore
    }

    public bool IsPickedUp()
    {
        return false;
    }

    public void AddFollower(AiBehavior newMinion)
    {
        if (newMinion != null)
        {
            if (newMinion.GetFaction() == GetFaction())
                AddMinionToFollowerList(newMinion);
        }
        
    }

    public bool IsMinionAlreadyFollowing(AiBehavior minion)
    {
        return _activeFollowers.Contains(minion);
    }

    public bool IsReadyForPickup()
    {
        return false;
    }

    public void TakeDamage(ITargetable aggressor, int damage)
    {
        if (!_isInvincible)
        {
            _health -= damage;

            //Take damage
            _bodyAnimator.SetBool("isDamaged", true);

            //reset the damaged anim (staying away from animation triggers ^_^)
            Invoke(nameof(ResetDamagedAnimation), _damagedAnimResetDelay);


            if (_health <= 0)
            {
                Die();
                _gameManager.TriggerGameLose("You Died");
            }
                

            //enter invinc to avoid too-frequent frame-after-frame hits
            EnterInvincAfterHit();
        }
    }



    public bool IsDead()
    {
        return _isDead;
    }

    public void RemoveFollower(AiBehavior minion)
    {
        if (IsMinionAlreadyFollowing(minion))
            _activeFollowers.Remove(minion);
    }

    public int GetHealth()
    {
        return _health;
    }

    public void SetGameManager(GameManager gm)
    {
        _gameManager = gm;
    }



    //DEbugging
}
