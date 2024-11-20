using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;




public class FootDisplacementManager : MonoBehaviour
{
    //Declarations
    [SerializeField] private List<FootDisplacement> _displacersList = new();
    [SerializeField] private bool _isDisplacementEnabled = true;


    //Monobehaviours





    //Internals
    private void ApplyMovementToFeets(Vector3 movementVector)
    {
        foreach (FootDisplacement displacer in _displacersList)
            displacer.NegateMovement(movementVector);
    }




    //Externals
    public void EnabledDisplacement(bool newState)
    {
        _isDisplacementEnabled = newState;

        foreach (FootDisplacement displacer in _displacersList)
            displacer.EnableFootDisplacement(_isDisplacementEnabled);
    }

    public void ApplyOffsetVelocityToFeets(Vector3 velocity)
    {
        ApplyMovementToFeets(velocity);
    }



}
