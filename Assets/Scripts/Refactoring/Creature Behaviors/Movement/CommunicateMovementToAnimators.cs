using UnityEngine;
using System.Collections.Generic;

public class CommunicateMovementToAnimators : MonoBehaviour
{
    [SerializeField] private HeadTurning _headTurnAnimator;
    [SerializeField] private List<FootDisplacement> _feetAnimators;

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
}
