using Unity.VisualScripting;
using UnityEngine;


public class HeadTurning : MonoBehaviour
{
    //Declarations
    [SerializeField] private Transform _bodyTransform;
    [SerializeField] private float _turnSpeed = 30;
    [SerializeField] private float _maxTurnRotation = 75;
    private Vector3 _targetVectorDirection;
    private bool _isTurningHead = true;

    [SerializeField] private Color _headRayColor = Color.yellow;
    [SerializeField] private Color _destinationColor = Color.red;
    [SerializeField] private Color _bodyRayColor = Color.magenta;


    //Monobehaviours
    private void Update()
    {
        TurnHead();
    }

    private void OnDrawGizmosSelected()
    {
        DrawHeadForwardsLine();
        DrawBodyForwardsLine();
        DrawTargetDestinationLine();
    }


    //Internals
    private void DrawBodyForwardsLine()
    {
        Gizmos.color = _bodyRayColor;
        Gizmos.DrawLine(_bodyTransform.position, _bodyTransform.position + _bodyTransform.forward * 5);
    }

    private void DrawTargetDestinationLine()
    {
        Gizmos.color = _destinationColor;
        Gizmos.DrawLine(transform.position, transform.position + _targetVectorDirection.normalized * 5);
    }

    private void DrawHeadForwardsLine()
    {
        Gizmos.color = _headRayColor;
        Gizmos.DrawLine(transform.position,transform.position+ transform.forward * 4);
    }


    private void TurnHead()
    {
        if (_isTurningHead)
            TurnViaVectorDirection();
    }

    private void TurnViaVectorDirection()
    {
        //calculate the angle from the targetVector's direction to head's forwards direction
        float signedAngleToDestination = Vector3.SignedAngle(transform.forward, _targetVectorDirection, Vector3.up);

        //calculate the angle from the body's forwards direction to head's forwards direction
        float signedAngleFromBody = Vector3.SignedAngle(_bodyTransform.forward, transform.forward, Vector3.up);


        //Are we turning towards the right (while remaining within our head's range of rotation)?
        if (signedAngleToDestination > 0 && signedAngleFromBody < _maxTurnRotation)
        {
            //Calculate a rotation additive that's to the right (+1)
            Vector3 additiveRotation = Vector3.up * Time.deltaTime * _turnSpeed;

            //apply the rotation to the object
            transform.eulerAngles += additiveRotation;
        }

        //Or are we turning towards the left (while remaining within our head's range of rotation)?
        else if (signedAngleToDestination < 0 && signedAngleFromBody > -_maxTurnRotation)
        {
            //Calculate a rotation additive that's to the right (+1)
            Vector3 additiveRotation = Vector3.up * Time.deltaTime * _turnSpeed * -1;

            //apply the rotation to the object
            transform.eulerAngles += additiveRotation;
        }
        
    }


    //Externals
    public void SetTargetDirection(Vector3 targetDirection)
    {
        _isTurningHead = true;
        _targetVectorDirection = targetDirection;
    }

    public void ReturnToNeutral()
    {
        _isTurningHead = true;
        _targetVectorDirection = _bodyTransform.forward;
    }

    public void StopTurningHead()
    {
        _isTurningHead = false;
    }




}
