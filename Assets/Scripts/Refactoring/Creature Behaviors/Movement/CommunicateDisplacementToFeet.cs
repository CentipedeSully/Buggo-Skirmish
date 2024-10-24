using UnityEngine;
using System.Collections.Generic;

public class CommunicateDisplacementToFeet : MonoBehaviour
{
    [SerializeField] private List<FootDisplacement> _feetAnimators;

    public void MoveFeetViaDisplacement(Vector3 frameDisplacement)
    {
        foreach (FootDisplacement footAnimator in _feetAnimators)
            footAnimator.NegateMovement(frameDisplacement);
    }
}
