using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Windows;



public class PlayerBehavior : MonoBehaviour, ITargetable
{
    //Declarations
    [SerializeField] private PlayerInput _playerInputReference;
    [SerializeField] private Vector2 _moveInput;
    [SerializeField] private bool _callAllNearbyUnitsInput;
    [SerializeField] private float _signalRadius = 10f;
    [SerializeField] private List<AiBehavior> _followers;
    [SerializeField] private Color _signalRadiusGizmoColor = Color.green;




    //Monobehaviours
    private void Update()
    {
        ReadInput();
        MovePlayer();
    }


    private void OnDrawGizmos()
    {
        DrawSignalRadius();
    }


    //Internals
    private void ReadInput()
    {
        
    }

    private void MovePlayer()
    {

    }

    private void CallNearbyUnits()
    {
        Collider[] detections = Physics.OverlapSphere(transform.position, _signalRadius);

        foreach (Collider collider in detections)
        {
            ITargetable behavior = collider.gameObject.GetComponent<ITargetable>();

            if (behavior != null)
            {
                //if this object is an ally minion
                if (behavior.GetFaction() == Faction.Ally && behavior.GetInteractableType() == InteractableType.Minion)
                {
                    //create a new non-interface variable
                    AiBehavior aiBehavior = (AiBehavior)behavior;

                    //follow us
                    aiBehavior.SetPursuitTarget(this);

                    //add this follower as a follower
                    _followers.Add(aiBehavior);
                }
            }
        }
    }

    private void DrawSignalRadius()
    {
        Gizmos.color = _signalRadiusGizmoColor;
        Gizmos.DrawWireSphere(transform.position,_signalRadius);
    }



    //Externals

    public void ReadCallUnitsInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            CallNearbyUnits();
    }

    public int GetBehaviorID()
    {
        return GetInstanceID();
    }

    public InteractableType GetInteractableType()
    {
        return InteractableType.Player;
    }

    public Faction GetFaction()
    {
        return Faction.Ally;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public int Nutrition()
    {
        return 0;
    }

    public void TriggerGravity()
    {
        //ignore
    }






    //DEbugging
}
