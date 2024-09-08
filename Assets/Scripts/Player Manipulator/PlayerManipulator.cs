using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.AI;


public class PlayerManipulator : MonoBehaviour
{
    //Declarations
    [Header("References")]
    [SerializeField] private Camera _mainCam;

    [Header("Settings")]
    [SerializeField] private float _maxCastDistance = 45f;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private NavMeshAgent _currentAgent;

    [Header("Detection Results")]
    [SerializeField] private bool _isGroundDetected = false;
    [SerializeField] private Vector3 _detectedGroundPosition;
    [SerializeField] private GameObject _detectedObject;

    [Header("Debug")]
    [SerializeField] private bool _drawMouseRay = true;
    [SerializeField] private Color _mouseRayColor = Color.magenta;



    //Monobehaviours
    private void OnDrawGizmos()
    {
        DrawMouseRaycast();
    }




    //Internals
    private void CommandMove(Vector3 position)
    {
        if (_currentAgent != null)
            _currentAgent.SetDestination(position);
    }

    private void CastMouseRay()
    {
        //clear outdated detection data
        _detectedObject = null;
        _isGroundDetected = false;

        //get our mouse screen position
        Vector3 mouseScreenPosition = Input.mousePosition;

        //build a ray using our mainCam and mousePosition
        Ray mouseRay = _mainCam.ScreenPointToRay(mouseScreenPosition);

        //raycast using our ray, and get any detections
        RaycastHit[] detections = Physics.RaycastAll(mouseRay, _maxCastDistance, _layerMask);

        //leave if nothing was found
        if (detections.Length == 0)
            return;

        //Get the index of the closest detection
        int closestIndex = detections.Length - 1;

        //save the first (closest) detection only
        _detectedObject = detections[closestIndex].collider.gameObject;

        //save the contact point if we hit the ground
        if (_detectedObject.CompareTag("Ground"))
        {
            _isGroundDetected = true;
            _detectedGroundPosition = detections[closestIndex].point;
        }

        else
            _isGroundDetected= false;
            

    }

    private void DrawMouseRaycast()
    {
        if (_drawMouseRay)
        {
            //get our mouse screen position
            Vector3 mouseScreenPosition = Input.mousePosition;

            //build a ray using our mainCam and mousePosition
            Ray mouseRay = _mainCam.ScreenPointToRay(mouseScreenPosition);

            //color the gizmo
            Gizmos.color = _mouseRayColor;

            //Draw the gizmo
            Gizmos.DrawRay(mouseRay.origin, mouseRay.direction * 100);
        }
    }


    //Externals
    public void CommandMoveOnInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //Capture our world-mouse position
            CastMouseRay();

            //move our current agent if our raycast hit the ground
            if (_isGroundDetected)
                CommandMove(_detectedGroundPosition);
        }
    }




    //Debugging





}
