using Sirenix.OdinInspector;
using UnityEngine;





public enum HeadPitchState
{
    unset,
    neutral,
    adjusting,
    raised
}

public class HeadPitchController : MonoBehaviour
{
    //Declarations
    [TabGroup("Pitch Controller", "Setup")]
    [SerializeField][Range(0,359)] private int _targetRaisedAngle = 30;
    [TabGroup("Pitch Controller", "Setup")]
    [SerializeField][Range(0, 359)] private int _targetNeutralAngle = 0;
    [TabGroup("Pitch Controller", "Setup")]
    [SerializeField] private float _angleForgiveness = .5f;
    [TabGroup("Pitch Controller", "Info")]
    [SerializeField] private float _pitchSpeed = 30f;
    [TabGroup("Pitch Controller", "Info")]
    [SerializeField] private HeadPitchState _currentPitch = HeadPitchState.unset;
    private HeadPitchState _targetPitch = HeadPitchState.unset;




    //Monobehaviours
    private void Start()
    {
        SetPitch(HeadPitchState.neutral);
    }

    private void Update()
    {
        AdjustHeadPitch();
    }




    //Internals
    private void AdjustHeadPitch()
    {
        if (_currentPitch == HeadPitchState.adjusting)
        {
            float xRotation = transform.rotation.eulerAngles.x;
            float yRotation = transform.rotation.eulerAngles.y;
            float zRotation = transform.rotation.eulerAngles.z;

            float xRotationAdditive = _pitchSpeed * Time.deltaTime;
            float xNewRotation;

            if (_targetPitch == HeadPitchState.neutral)
            {
                //increase or decrease the angle based on our current value
                if (xRotation > _targetNeutralAngle)
                    xNewRotation = xRotation - xRotationAdditive;

                else if (xRotation < _targetNeutralAngle)
                    xNewRotation = xRotation + xRotationAdditive;

                else
                    xNewRotation = xRotation;

                //set the newRotation to our target if it's within the angle forgiveness range
                if (Mathf.Clamp(xNewRotation, _targetNeutralAngle - _angleForgiveness, _targetNeutralAngle + _angleForgiveness) == xNewRotation)
                    xNewRotation = _targetNeutralAngle;

                transform.rotation = Quaternion.Euler(xNewRotation, yRotation, zRotation);

                if (xNewRotation == _targetNeutralAngle)
                {
                    //update our states
                    _targetPitch = HeadPitchState.unset;
                    _currentPitch = HeadPitchState.neutral;
                }
            }

            else if (_targetPitch == HeadPitchState.raised)
            {
                //increase or decrease the angle based on our current value
                if (xRotation > _targetRaisedAngle)
                    xNewRotation = xRotation - xRotationAdditive;

                else if (xRotation < _targetRaisedAngle)
                    xNewRotation = xRotation + xRotationAdditive;

                else
                    xNewRotation = xRotation;

                //set the newRotation to our target if it's within the angle forgiveness range
                if (Mathf.Clamp(xNewRotation, _targetRaisedAngle - _angleForgiveness, _targetRaisedAngle + _angleForgiveness) == xNewRotation)
                    xNewRotation = _targetRaisedAngle;

                transform.rotation = Quaternion.Euler(xNewRotation, yRotation, zRotation);

                if (xNewRotation == _targetRaisedAngle)
                {
                    //update our states
                    _targetPitch = HeadPitchState.unset;
                    _currentPitch = HeadPitchState.raised;
                }
            }
        }
    }




    //Externals
    [TabGroup("Pitch Controller", "Debug")]
    [Button]
    public void InterruptAdjustment()
    {
        if (_currentPitch == HeadPitchState.adjusting)
        {
            _targetPitch = HeadPitchState.unset;
            _currentPitch = HeadPitchState.unset;
        }
    }

    [TabGroup("Pitch Controller", "Debug")]
    [Button]
    public void SetPitch(HeadPitchState newState)
    {
        if (newState != _currentPitch && 
            newState != HeadPitchState.unset && 
            newState != HeadPitchState.adjusting)
        {
            _targetPitch = newState;
            _currentPitch = HeadPitchState.adjusting;
        }
    }

    public HeadPitchState GetHeadPitchState() { return _currentPitch; }






}
