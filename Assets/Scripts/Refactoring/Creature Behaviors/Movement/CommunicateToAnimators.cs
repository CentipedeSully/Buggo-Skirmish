using UnityEngine;
using System.Collections.Generic;

public class CommunicateToAnimators : MonoBehaviour
{
    [SerializeField] private LookAtController _lookAtController;
    [SerializeField] private FootDisplacementManager _footDisplacementManager;
    [SerializeField] private MandiblesController _mandiblesController;
    [SerializeField] private Vector3 _minionHeadPickupOffset;


    public void TurnHead(Vector3 moveDirection)
    {
        _lookAtController.LookAtPosition(moveDirection);
    }

    public void MoveFeetViaDisplacement(Vector3 moveDirection)
    {
        if (_footDisplacementManager != null)
            _footDisplacementManager.ApplyOffsetVelocityToFeets(moveDirection);
    }

    public void ReturnHeadToNeutral()
    {
        _lookAtController.ReturnHeadToNeutral();
    }

    public void StopTurningHead()
    {
        _lookAtController.InterruptCurrentTransition();
    }

    public void SetMandibles(JawState newState) { _mandiblesController.SetJawState(newState); }

    public void InterruptMandibles() { _mandiblesController.InterruptTransition(); }

    public void InterruptAllAnimators()
    {
        StopTurningHead();
        InterruptMandibles();
    }

    public void SetPickupAnimation(bool isCarrying, CreatureType creatureType)
    {
        //Minions tilt their heads up when carrying something
        //Other creatures may behave differently...
        if (creatureType == CreatureType.minion)
        {
            if (isCarrying)
                _lookAtController.SetOffset(_minionHeadPickupOffset);

            else
                _lookAtController.ClearOffset();
        }
    }
}
