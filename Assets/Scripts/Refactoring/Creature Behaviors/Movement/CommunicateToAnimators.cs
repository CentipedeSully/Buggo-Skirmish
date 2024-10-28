using UnityEngine;
using System.Collections.Generic;

public class CommunicateToAnimators : MonoBehaviour
{
    [SerializeField] private HeadTurning _headTurnAnimator;
    [SerializeField] private List<FootDisplacement> _feetAnimators;
    [SerializeField] private HeadPitchController _headPitchController;
    [SerializeField] private MandiblesController _mandiblesController;

    public void MoveFeetViaDisplacement(Vector3 frameDisplacement)
    {
        foreach (FootDisplacement footAnimator in _feetAnimators)
            footAnimator.NegateMovement(frameDisplacement);
    }

    public void TurnHead(Vector3 moveDirection)
    {
        _headTurnAnimator.SetTargetDirection(moveDirection);
    }

    public void ReturnHeadToNeutral()
    {
        _headTurnAnimator.ReturnToNeutral();
    }

    public void StopTurningHead()
    {
        _headTurnAnimator.StopTurningHead();
    }

    public void SetMandibles(JawState newState) { _mandiblesController.SetJawState(newState); }

    public void InterruptMandibles() { _mandiblesController.InterruptTransition(); }

    public void SetHeadPitch(HeadPitchState newState) { _headPitchController.SetPitch(newState); }

    public void InterruptHeadPitching() { _headPitchController.InterruptAdjustment(); }

    public void InterruptAllAnimators()
    {
        StopTurningHead();
        InterruptHeadPitching();
        InterruptMandibles();
    }
}
