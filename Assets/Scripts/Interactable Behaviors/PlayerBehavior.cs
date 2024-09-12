using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Windows;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;



public class PlayerBehavior : MonoBehaviour, ITargetable
{
    //Declarations
    [SerializeField] private Camera _mainCam;
    [SerializeField] private Transform _cameraParent;
    [SerializeField] private PlayerInput _playerInputReference;
    [SerializeField] private PlayerManipulator _playerManipulator;
    [SerializeField] private Transform _bodyModelTransform;
    private InputAction _moveInputAction;
    private InputAction _callUnitsInputAction;
    private InputAction _mouseMovementInputAction;
    private InputAction _mouseLclickInputAction;
    private InputAction _mouseRclickInputAction;


    private Vector2 _moveInput;
    private float _cameraInput;
    private bool _callUnitsInput;

    [SerializeField] private float _moveSpeed = 3;
    [SerializeField] private float _cameraRotationSpeed = 20f;
    [SerializeField] private float _signalRadius = 10f;
    [SerializeField] private Color _signalRadiusGizmoColor = Color.green;
    [SerializeField] private float _actionCooldownDuration = .1f;
    private bool _isUnitCallReady = true;
    

    private bool _isLerpingBody = false;
    [SerializeField]private float _bodyRotationLerpDuration = .3f;
    private float _currentRotationTime = 0;
    private Vector3 _startLookatVector;
    private Vector3 _targetLookAtVector;
    private Vector3 _currentLookAtVector;

    private Vector3 _cameraRelativeMoveDirection;
    private Vector3 _worldMoveVector;
    [SerializeField] private Color _moveDirectionGizmoColor = Color.cyan;
    [SerializeField] private float _gizmoLineLength = 5;

    [SerializeField] private List<AiBehavior> _activeFollowers;
    private bool _isDead;

    [SerializeField] private Animator _bodyAnimator;
    [SerializeField] private float _damagedAnimResetDelay = .1f;
    [SerializeField] private bool _isInvincible = false;
    [SerializeField] private float _invincTimeAfterHit = .1f;





    //Monobehaviours
    private void Start()
    {
        //make the player ignore the idel and move anims (they break, currently when the player turns)
        _bodyAnimator.SetBool("isPlayer", true);

        //defualt the _currentLookAtDirection
        _currentLookAtVector = transform.position + transform.TransformDirection(Vector3.forward);

        //Collect action references
        _moveInputAction = _playerInputReference.actions.FindAction("PlayerMovement");
        _callUnitsInputAction = _playerInputReference.actions.FindAction("Call Units");
        _mouseMovementInputAction = _playerInputReference.actions.FindAction("CameraMovement");
        _mouseLclickInputAction = _playerInputReference.actions.FindAction("LClick");
        _mouseRclickInputAction = _playerInputReference.actions.FindAction("RClick");
    }

    private void Update()
    {
        ReadInput();
        MovePlayer();
        LerpBodyRotationTowardsLookAtPosition();
        
        TurnCamera();
        CallNearbyUnits();
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
            GiveCommandViaManipulatorContext();

        
    }

    private void LerpBodyRotationTowardsLookAtPosition()
    {
        if (_isLerpingBody)
        {
            //update the time
            _currentRotationTime += Time.deltaTime;

            //blend the current lookat position into the target vector over time
            _currentLookAtVector = Vector3.Lerp(_startLookatVector, _targetLookAtVector, _currentRotationTime / _bodyRotationLerpDuration);

            //make the player's body rotate towards the current lookAt position
            _bodyModelTransform.LookAt(_currentLookAtVector);

            //stop lerping if we've reached out target position
            if (_currentLookAtVector == _targetLookAtVector)
            {
                _isLerpingBody = false;
                _currentRotationTime = 0;

            }


        }
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

            //move the player
            transform.Translate(_moveSpeed * Time.deltaTime * _cameraRelativeMoveDirection);

            //are we NOT currently looking
            if (_currentLookAtVector.normalized != _worldMoveVector.normalized)
            {
                //Setup the rotation-over-time utilities 
                _targetLookAtVector = _worldMoveVector;
                _startLookatVector = new Vector3(_currentLookAtVector.x, transform.position.y,_currentLookAtVector.z); //make sure the vector is level
                _currentRotationTime = 0;

                //start rotating the player's body towards the move direction
                _isLerpingBody = true;
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

            //Ally?
            if (target.GetFaction() == Faction.Ally)
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
        //Take damage
        _bodyAnimator.SetBool("isDamaged", true);

        //reset the damaged anim (staying away from animation triggers ^_^)
        Invoke(nameof(ResetDamagedAnimation), _damagedAnimResetDelay);

        //enter invinc to avoid too-frequent frame-after-frame hits
        EnterInvincAfterHit();
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



    //DEbugging
}
