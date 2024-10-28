using Sirenix.OdinInspector;
using UnityEngine;




public enum JawState
{
    unset,
    open,
    closed
}

public class MandiblesController : MonoBehaviour
{
    //Declarations
    [TabGroup("Mandibles Controller", "Setup")]
    [SerializeField] private Transform _lMandible;
    [TabGroup("Mandibles Controller", "Setup")]
    [SerializeField] private Transform _rMandible;
    [TabGroup("Mandibles Controller", "Setup")]
    [SerializeField] private Transform _lOpenTransform;
    [TabGroup("Mandibles Controller", "Setup")]
    [SerializeField] private Transform _rOpenTransform;
    [TabGroup("Mandibles Controller", "Setup")]
    [SerializeField] private Transform _lClosedTransform;
    [TabGroup("Mandibles Controller", "Setup")]
    [SerializeField] private Transform _rClosedTransform;


    [TabGroup("Mandibles Controller", "Info")]
    [SerializeField] private JawState _jawState = JawState.unset;
    private JawState _targetState = JawState.unset;

    [TabGroup("Mandibles Controller", "Info")]
    [SerializeField] private float _transitionDuration = .2f;
    private bool _isTransitioning = false;
    private float _currentTransitionTime = 0f;



    //Monobehaviours
    private void Start()
    {
        SetJawState(JawState.open);
    }

    private void Update()
    {
        LerpMandibles();
    }




    //Internals
    private void LerpMandibles()
    {
        if (_isTransitioning)
        {
            //Tick the time
            _currentTransitionTime += Time.deltaTime;

            //cache the normalized time for clarity
            float currentNormalizedTime = _currentTransitionTime / _transitionDuration;

            //either lerp mandibles to the open state
            if (_targetState == JawState.open)
            {
                _rMandible.transform.position = Vector3.Lerp(_rClosedTransform.position, _rOpenTransform.position, currentNormalizedTime);
                _lMandible.transform.position = Vector3.Lerp(_lClosedTransform.position, _lOpenTransform.position, currentNormalizedTime);
                _lMandible.transform.rotation = Quaternion.Lerp(_lClosedTransform.rotation, _lOpenTransform.rotation, currentNormalizedTime);
                _rMandible.transform.rotation = Quaternion.Lerp(_rClosedTransform.rotation, _rOpenTransform.rotation, currentNormalizedTime);
            }

            //or lerp mandibles to the closed state
            else if (_targetState == JawState.closed)
            {
                _rMandible.transform.position = Vector3.Lerp(_rOpenTransform.position, _rClosedTransform.position, currentNormalizedTime);
                _lMandible.transform.position = Vector3.Lerp(_lOpenTransform.position, _lClosedTransform.position, currentNormalizedTime);
                _lMandible.transform.rotation = Quaternion.Lerp(_lOpenTransform.rotation, _lClosedTransform.rotation, currentNormalizedTime);
                _rMandible.transform.rotation = Quaternion.Lerp(_rOpenTransform.rotation, _rClosedTransform.rotation, currentNormalizedTime);
            }

            //end the lerp if our time has expired 
            if (_currentTransitionTime >= _transitionDuration)
            {
                //update the state!
                _jawState = _targetState;

                //reset utils
                _isTransitioning = false;
                _currentTransitionTime = 0;
                _targetState = JawState.unset;
            }
        }
    }

    private float CalculateStartingLerpTime(Transform lStartTransform, Transform rStartTransform)
    {
        //offset the current time if our mandibles aren't starting from the expected positions
        Transform lExpectedStartTransform;
        Transform rExpectedStartTransform;
        Transform lEndTransform;
        Transform rEndTransform;
        float lCurrentlyCompletedNormalizedProgress;
        float rCurrentlyCompletedNormalizedProgress;

        //Determine our expected startPositions
        if (_targetState == JawState.open)
        {
            lExpectedStartTransform = _lClosedTransform;
            rExpectedStartTransform = _rClosedTransform;

            lEndTransform = _lOpenTransform;
            rEndTransform = _rOpenTransform;
        }
        else
        {
            lExpectedStartTransform = _lOpenTransform;
            rExpectedStartTransform = _rOpenTransform;

            lEndTransform = _lClosedTransform;
            rEndTransform = _rClosedTransform;
        }

        //Are we lerping the mandible's positions?
        //Infer this based on whether or not there's any change from startPosition to endPosition
        if (rExpectedStartTransform.position != rEndTransform.position ||
            lExpectedStartTransform.position != lEndTransform.position)
        {
            //Calculate our distances
            float lMaxDistance = Vector3.Distance(lExpectedStartTransform.position, lEndTransform.position);
            float rMaxDistance = Vector3.Distance(rExpectedStartTransform.position, rEndTransform.position);

            float lCurrentDistance = Vector3.Distance(lStartTransform.position, lEndTransform.position);
            float rCurrentDistance = Vector3.Distance(rStartTransform.position, rEndTransform.position);


            //calulate the already-completed progress (normalized) for each mandible.
            rCurrentlyCompletedNormalizedProgress = 1 - rCurrentDistance / rMaxDistance;
            lCurrentlyCompletedNormalizedProgress = 1 - lCurrentDistance / lMaxDistance;

            //take the mandible with the least time completed
            float currentNormalizedProgress = Mathf.Min(rCurrentlyCompletedNormalizedProgress, lCurrentlyCompletedNormalizedProgress);
            //Debug.Log($"Normalized Distance: {currentNormalizedProgress}");

            //return the derived 'startingTime' that'll be used in place of starting at 0;
            return currentNormalizedProgress * _transitionDuration;
        }

        //Assume we're lerping the mandible's rotations, since no changes btwn the start and end positions were found
        else
        {
            //Calculate our distances
            float lMaxDistance = Vector3.Distance(lExpectedStartTransform.rotation.eulerAngles, lEndTransform.rotation.eulerAngles);
            float rMaxDistance = Vector3.Distance(rExpectedStartTransform.rotation.eulerAngles, rEndTransform.rotation.eulerAngles);

            float lCurrentDistance = Vector3.Distance(lStartTransform.rotation.eulerAngles, lEndTransform.rotation.eulerAngles);
            float rCurrentDistance = Vector3.Distance(rStartTransform.rotation.eulerAngles, rEndTransform.rotation.eulerAngles);

            //calulate the already-completed progress (normalized) for each mandible.
            rCurrentlyCompletedNormalizedProgress = 1 - rCurrentDistance / rMaxDistance;
            lCurrentlyCompletedNormalizedProgress = 1 - lCurrentDistance / lMaxDistance;

            //take the mandible with the least time completed
            float currentNormalizedProgress = Mathf.Min(rCurrentlyCompletedNormalizedProgress, lCurrentlyCompletedNormalizedProgress);
            //Debug.Log($"Normalized Distance: {currentNormalizedProgress}");

            //return the derived 'startingTime' that'll be used in place of starting at 0;
            return currentNormalizedProgress * _transitionDuration;
        }
    }




    //Externals
    public JawState GetJawState() { return _jawState; }

    [TabGroup("Mandibles Controller", "Debug")]
    [Button]
    public void InterruptTransition() 
    { 
        if (_isTransitioning)
        {
            _isTransitioning = false;
            _currentTransitionTime = 0;


            _jawState = JawState.unset;
            _targetState = JawState.unset;
        }
    }

    [TabGroup("Mandibles Controller", "Debug")]
    [Button]
    public void SetJawState(JawState newState)
    {
        if (newState != _jawState && newState != JawState.unset)
        {
            if (_isTransitioning)
                InterruptTransition();

            //set target state
            _targetState = newState;


            //Calculate the startingTime based on the mandible's current positions relative to their expected positions
            _currentTransitionTime = CalculateStartingLerpTime(_lMandible, _rMandible);
            //Debug.Log($"BeginningTransition. Calculated Start Time: {_currentTransitionTime}");

            //begin transitioning over time
            _isTransitioning = true;
        }
    }



}
