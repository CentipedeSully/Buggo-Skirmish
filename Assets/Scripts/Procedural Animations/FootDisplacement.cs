using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FootDisplacement : MonoBehaviour
{
    //Declarations
    [SerializeField] private FootDisplacement _oppositeFoot;
    [SerializeField] private float _maxFootDistance = .2f;
    [SerializeField] private float _displaceDuration = .2f;
    private float _currentDisplaceDuration = 0;
    private bool _isLerping = false;
    [SerializeField] private Transform _foot;
    [SerializeField] private Transform _target;




    //Monobehaviours
    private void Update()
    {
        LerpFootToTargetIfDistanceTooGreat();
    }




    //Internals
    private bool IsDistanceTooGreat()
    {
        float distance = Vector3.Distance(_foot.position, _target.position);
        return _maxFootDistance < distance;
    }

    private void CheckFootDistance()
    {
        if (IsDistanceTooGreat() && !_isLerping && _oppositeFoot.IsGrounded())
            _isLerping = true;
    }

    private void LerpFootToTargetIfDistanceTooGreat()
    {
        if (_isLerping)
        {
            //tick the displacement duration
            _currentDisplaceDuration += Time.deltaTime;

            //move the foot
            _foot.position = Vector3.Lerp(_foot.position, _target.position, _currentDisplaceDuration / _displaceDuration);

            //reset the lerp if it got completed
            if (_currentDisplaceDuration >= _displaceDuration)
            {
                _isLerping = false;
                _currentDisplaceDuration = 0;
            }
        }
    }


    //Externals
    public void NegateMovement(Vector3 currentFrameDisplacement)
    {
        if (!_isLerping)
        {
            _foot.position -= currentFrameDisplacement;
            CheckFootDistance();
        }
    }

    public bool IsGrounded()
    {
        return !_isLerping;
    }





}
