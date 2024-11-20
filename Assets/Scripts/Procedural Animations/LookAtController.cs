using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class LookAtController : MonoBehaviour
{
    //Declarations
    [SerializeField] private Transform _lookAtTarget;
    [SerializeField] private Vector3 _defaultLookPosition;
    [SerializeField] private float _lookAtDistance = 4;
    [SerializeField] private Vector3 _offset = Vector3.zero;
    [SerializeField] private float _distanceForgiveness = .1f;
    [SerializeField] private float _transitionSpeed = .2f;
    private float _currentTransitionDuration = 0;
    private Vector3 _targetLocalPosition;
    private Vector3 _startLocalPosition;
    private Vector3 _targetWorldPos;
    private Vector3 _startWorldPos;
    [ReadOnly]
    [SerializeField] private bool _isLerping = false;





    //Monobehaviours
    private void Awake()
    {
        _defaultLookPosition = transform.forward;
        LookAtPosition(_defaultLookPosition);
    }

    private void Update()
    {
        LerpLookAtPosition();
    }


    //Internals
    private void LerpLookAtPosition()
    {
        if (_isLerping)
        {
            //tick time
            _currentTransitionDuration += Time.deltaTime;

            //Apply the lerp
            _lookAtTarget.position = Vector3.Lerp(_startWorldPos, _targetWorldPos, _currentTransitionDuration / _transitionSpeed);

            //snap to our position if we're within the distance forgiveness's range
            if (Vector3.Distance(_lookAtTarget.position, _targetWorldPos) <= _distanceForgiveness)
                _lookAtTarget.position = _targetWorldPos;
            

            //Is our time expired?
            if (_currentTransitionDuration >= _transitionSpeed)
            {
                _currentTransitionDuration = 0;
                _isLerping = false;
            }
            
        }
    }


    //Externals
    public void ReturnHeadToNeutral() { LookAtPosition(_defaultLookPosition); }

    public void SetOffset(Vector3 offset) { _offset = offset; }

    public void ClearOffset() { SetOffset(Vector3.zero); }

    public Vector3 GetOffset() { return _offset; }

    public void LookAtPosition(Vector3 direction)
    {
        //clarify our calculations
        Vector3 currentLocalPosition = transform.InverseTransformPoint(_lookAtTarget.position);
        Vector3 desiredLocalPosition = transform.InverseTransformDirection(direction.normalized * _lookAtDistance) + _offset;

        //Debug.Log($"LookAt Local: {currentLocalPosition}\nDesired Local: {desiredLocalPosition}");

        //Stop transitioning if we're mid transition
        if (currentLocalPosition == desiredLocalPosition && _isLerping)
            InterruptCurrentTransition();

        else
        {
            //are we mid transition?
            if (_isLerping)
            {
                //is our current target position different from our new desired position
                if (_targetLocalPosition != desiredLocalPosition)
                {
                    //Stop following the old, outdated transition
                    InterruptCurrentTransition();

                    //Start a new Transition
                    LookAtPosition(direction);
                }
            }

            //Start a new transition
            else
            {
                _startLocalPosition = currentLocalPosition;
                _targetLocalPosition = desiredLocalPosition;

                _startWorldPos = transform.TransformPoint(_startLocalPosition);
                _targetWorldPos = transform.TransformPoint(_targetLocalPosition);

                _isLerping = true;
            }
        }
    }

    public void InterruptCurrentTransition()
    {
        if (_isLerping)
        {
            _isLerping = false;

            //set the old target as where we're stopping now
            _targetLocalPosition = transform.InverseTransformPoint(_lookAtTarget.position);
            _currentTransitionDuration=0;
        }
    }





}
