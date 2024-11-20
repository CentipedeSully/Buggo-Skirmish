using Sirenix.OdinInspector;
using UnityEngine;

public class LimbRebounder : MonoBehaviour
{
    //Declarations
    private Vector3 _origin;
    [ReadOnly] private Vector3 _bounceDirection;
    [SerializeField] private float _magnitude;
    [SerializeField] private AnimationCurve _normalizedMagnitudeAnimationCurve;
    [ReadOnly] [SerializeField] private bool _isRebounding = false;
    [SerializeField] private float _duration = .5f;

    private float _currentTime;
    private float _normalizedTime;
    private float _instanceMagnitude;


    //Monobehaviours
    private void Awake()
    {
        _origin = transform.localPosition;
    }

    private void Update()
    {
        
        if (_isRebounding)
            ManageBounce();
    }



    //Internals
    private void ManageBounce()
    {
        //Tick the time
        _currentTime += Time.deltaTime;

        //get the respective normalized times
        _normalizedTime = _currentTime / _duration;

        //calculate our progress along the magnitude animation curve
        _instanceMagnitude = _normalizedMagnitudeAnimationCurve.Evaluate(_normalizedTime);

        //Apply the relevant magnitude to the given vector, and displace
        transform.localPosition = _origin + _bounceDirection.normalized * _instanceMagnitude;

        //exit the bounce-back if we've reached our duration
        if (_currentTime >= _duration)
        {
            _isRebounding = false;
            _currentTime = 0;
        }
    }




    //Externals
    public bool IsRebounding() { return _isRebounding; }

    [Button]
    public void ApplyBounceDirection(Vector3 direction, float magnitude)
    {
        //update our utils
        _bounceDirection = direction;
        _magnitude = magnitude;
        _currentTime = 0;

        //enter the bounce login on the next update
        _isRebounding = true;
    }




}
