using UnityEngine;




public interface ILimbAnimator
{
    LimbType LimbType { get; }
    AnimType AnimType { get; }

    void SetToNeutral();
    void StartAnim();
    void InterruptAnim();
    bool IsInProgress();
    float GetAnimDuration();
    float GetToNeutralDuration();

}

public enum LimbType
{
    unset,
    midBody,
    head,
    butt,
    mandibles,
    feet
}

public enum AnimType
{
    unset,
    idling,
    moving,
    preparingToStrike,
    striking,
    recoveringFromStrike
}

public class AnimationInterfaces : MonoBehaviour{}
