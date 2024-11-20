using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FootDisplacement : MonoBehaviour
{
    //Declarations
    [SerializeField] private FootDisplacement _oppositeFootDisplacementRef;
    [SerializeField] private float _maxFootDistance = .2f;
    [SerializeField] private float _displaceDuration = .2f;
    private float _currentDisplaceDuration = 0;
    private bool _isLerpInProgress = false;
    [SerializeField] private Transform _footConstraintSource;
    [SerializeField] private Transform _neutralPosition;
    [SerializeField] private bool _isDisplacementEnabled = true;



    //Monobehaviours
    private void Update()
    {
        if (_isDisplacementEnabled)
        {
            CheckFootDistance();
            LerpConstraintSourceToNeutralPositionIfDistanceTooGreat();
        }
    }




    //Internals
    private bool IsDistanceTooGreat()
    {
        float distance = Vector3.Distance(_footConstraintSource.position, _neutralPosition.position);
        return _maxFootDistance < distance;
    }

    private void CheckFootDistance()
    {
        if (!_isLerpInProgress)
        {
            if (IsDistanceTooGreat() && _oppositeFootDisplacementRef.IsGrounded())
                _isLerpInProgress = true;
        }
        
    }

    private void LerpConstraintSourceToNeutralPositionIfDistanceTooGreat()
    {
        if (_isLerpInProgress)
        {
            //tick the displacement duration
            _currentDisplaceDuration += Time.deltaTime;

            //move the foot
            _footConstraintSource.position = Vector3.Lerp(_footConstraintSource.position, _neutralPosition.position, _currentDisplaceDuration / _displaceDuration);

            //reset the lerp if it got completed
            if (_currentDisplaceDuration >= _displaceDuration)
            {
                _isLerpInProgress = false;
                _currentDisplaceDuration = 0;
            }
        }
    }


    //Externals
    public void NegateMovement(Vector3 currentFrameDisplacement)
    {
        if (!_isLerpInProgress)
            _footConstraintSource.position -= currentFrameDisplacement;
    }

    public bool IsGrounded()
    {
        return !_isLerpInProgress;
    }

    public void EnableFootDisplacement(bool newState)
    {
        _isDisplacementEnabled = newState;
    }

    public bool IsFootDisplacementEnabled() {  return _isDisplacementEnabled; }




}
