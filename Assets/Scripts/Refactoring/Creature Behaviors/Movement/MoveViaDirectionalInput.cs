using UnityEngine;
using Sirenix;
using UnityEngine.InputSystem;



public interface IPlayerMoveBehavior
{
    void ToggleMovement(bool newState);
    bool IsMovementEnabled();
    bool IsMoving();

}

public class MoveViaDirectionalInput : MonoBehaviour, IPlayerMoveBehavior, ICreatureBehavior
{
    //Declarations
    [Header("Move Utils")]
    [SerializeField] private bool _isMovementEnabled = true;
    [SerializeField] private bool _isMoving = false;
    private Vector2 _moveInput;
    private float _cameraInput;
    [SerializeField] private float _moveSpeed = 3;
    [SerializeField] private float _turnSpeed = 180;
    [SerializeField] private float _lookAngleTolerance = 10;
    [SerializeField] private float _cameraRotationSpeed = 20f;
    private Vector3 _cameraRelativeMoveDirection;
    private Vector3 _worldMoveVector;
    [SerializeField] private Color _moveDirectionGizmoColor = Color.cyan;
    [SerializeField] private float _gizmoLineLength = 5;
    


    [Header("References")]
    [SerializeField] private PlayerInput _playerInputReference;
    [SerializeField] private Transform _bodyModelTransform;
    [SerializeField] private Camera _shoulderCam;
    [SerializeField] private Transform _cameraPivot;
    private Rigidbody _rigidbody;
    private CommunicateDisplacementToFeet _feetDisplacer;
    private InputAction _moveInputAction;
    private InputAction _mouseMovementInputAction;





    //Monobehaviours
    private void Awake()
    {
        InitializeReferences();
    }

    private void Update()
    {
        ReadInput();
        TurnCamera();
        MoveObject();
    }

    private void OnDrawGizmosSelected()
    {
        DrawMoveDirectionGizmo();
    }



    //Internals
    private void InitializeReferences()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _feetDisplacer = GetComponent<CommunicateDisplacementToFeet>();

        //Collect action references
        _moveInputAction = _playerInputReference.actions.FindAction("PlayerMovement");
        _mouseMovementInputAction = _playerInputReference.actions.FindAction("CameraMovement");
    }

    private void ReadInput()
    {
        //Movement
        if (_moveInputAction.IsPressed())
            _moveInput = _moveInputAction.ReadValue<Vector2>();

        else
            _moveInput = Vector2.zero;
            

        //Mouse Movement
        if (_mouseMovementInputAction.IsPressed())
            _cameraInput = _mouseMovementInputAction.ReadValue<float>();
        else
            _cameraInput = 0;
    }

    private void TurnCamera()
    {
        if (_cameraInput != 0)
        {
            //get the old euler angle
            Vector3 oldRotation = _cameraPivot.transform.rotation.eulerAngles;

            //create the additive
            Vector3 additiveRotation = new Vector3(0, _cameraInput * _cameraRotationSpeed * Time.deltaTime, 0);

            //calculate the sum rotation
            Vector3 sumRotation = oldRotation - additiveRotation;

            //Apply the new rotation
            _cameraPivot.transform.eulerAngles = sumRotation;
        }

        _cameraPivot.transform.position = transform.position;
    }

    private void DrawMoveDirectionGizmo()
    {
        Gizmos.color = _moveDirectionGizmoColor;
        Gizmos.DrawLine(transform.position, transform.position + _gizmoLineLength * _cameraRelativeMoveDirection);
    }

    private void MoveObject()
    {
        if (_moveInput != Vector2.zero && _isMovementEnabled)
        {
            _isMoving = true;

            //create the raw move vector
            Vector3 moveVector = new Vector3(_moveInput.x, 0, _moveInput.y);

            //relate the raw vector into the camera's local space
            _cameraRelativeMoveDirection = _cameraPivot.transform.TransformDirection(moveVector);

            //calculate the properly-displaced move vector
            _worldMoveVector = transform.position + _cameraRelativeMoveDirection;

            //calculate our forwards direction
            Vector3 bodyForwardsVector = _bodyModelTransform.TransformVector(Vector3.forward);

            //Draw the forwards direction for debug purposes (if necessary)
            Debug.DrawRay(_bodyModelTransform.position, bodyForwardsVector * 5);

            //move the player
            transform.Translate(_moveSpeed * Time.deltaTime * _cameraRelativeMoveDirection);

            //Move the feets, too!
            if (_feetDisplacer != null)
                _feetDisplacer.MoveFeetViaDisplacement(_moveSpeed * Time.deltaTime * _cameraRelativeMoveDirection);

            //calculate the angle from the body forwards to themove vector
            float signedAngle = Vector3.SignedAngle(bodyForwardsVector, _cameraRelativeMoveDirection, Vector3.up);

            //Log the math, for clarity
            //Debug.Log($"Player Angle: {signedAngle}");

            //are we currently outside the specified angular tolerance
            if (signedAngle < -_lookAngleTolerance || signedAngle > _lookAngleTolerance)
            {
                //Calculate a rotation additive that's in the opposite direction of our signed angular difference
                Vector3 additiveRotation = Vector3.up * Time.deltaTime * _turnSpeed * Mathf.Sign(signedAngle);

                //apply the rotation to the object
                _bodyModelTransform.eulerAngles += additiveRotation;
            }
        }

        else
        {
            _isMoving = false;
        }
    }




    //Externals
    public bool IsMoving() { return _isMoving; }
    public bool IsMovementEnabled() { return _isMovementEnabled; }
    public void ToggleMovement(bool newState) {  _isMovementEnabled = newState; }

    public void ReadCreatureData(CreatureData data)
    {
        _moveSpeed = data.GetBaseMoveSpeed();
        _turnSpeed = data.GetBaseTurnSpeed();
    }

    public void InterruptBehavior()
    {
        throw new System.NotImplementedException();
    }
}
