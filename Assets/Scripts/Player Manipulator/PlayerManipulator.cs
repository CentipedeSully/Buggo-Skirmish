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

    private void Update()
    {
        DectectHoverables();
    }


    //Internals
    private void DectectHoverables()
    {
        //save the previous detection data
        GameObject lastDetection = _detectedObject;

        //Get new detection data
        CastMouseRay();

        //If we've detected nothing (or the ground), then exit the previous detection's hover state
        if (_detectedObject == null || (_detectedObject != null && _isGroundDetected))
        {
            //does a previous detection exist?
            if (lastDetection != null)
            {
                //is it NOT the ground?
                if (!lastDetection.CompareTag("Ground"))
                {
                    //get its targetable behavior
                    ITargetable lastDetectedBehavior = lastDetection.GetComponent<ITargetable>();

                    //Exit the behavior's hover state (if it exists)
                    lastDetectedBehavior?.OnHoverExited();
                }
            }
        }

        
        //Else if our detection isn't the ground && the last detection is different the current one
        else if (!_isGroundDetected && _detectedObject != lastDetection)
        {
            //does a previous detection exist?
            if (lastDetection != null)
            {
                //get its targetable behavior
                ITargetable lastDetectedBehavior = lastDetection.GetComponent<ITargetable>();

                //Exit the behavior's hover state (if it exists)
                lastDetectedBehavior?.OnHoverExited();
            }

            //get the new detection's behaivor
            ITargetable newDectectionBehavior = _detectedObject.GetComponent<ITargetable>();

            //enter hover on this new behavior
            newDectectionBehavior?.OnHoverEntered();
        }
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
    public void CastMouseRay()
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

        //prioritize Actor detections first
        foreach (RaycastHit detection in detections)
        {
            if (detection.collider.CompareTag("Actor"))
            {
                _detectedObject = detection.collider.gameObject;
                return;
            }
        }

        //if no actors were detected, try again and prioritize Nests detections 
        if (_detectedObject == null)
        {
            foreach (RaycastHit detection in detections)
            {
                if (detection.collider.CompareTag("Nest"))
                {
                    _detectedObject = detection.collider.gameObject;
                    return;
                }
            }
        }
        

        //if no actors were detected, try again and prioritize finding pickups
        if (_detectedObject == null)
        {
            foreach (RaycastHit detection in detections)
            {
                if (detection.collider.CompareTag("Pickup"))
                {
                    _detectedObject = detection.collider.gameObject;
                    return;
                }
            }
        }


        //if we also failed to detect any pickups, then it's probably just floor here
        if (_detectedObject == null)
        {
            foreach (RaycastHit detection in detections)
            {
                if (detection.collider.CompareTag("Ground"))
                {
                    _detectedObject = detection.collider.gameObject;

                    //save the contact point
                    _isGroundDetected = true;
                    _detectedGroundPosition = detection.point;
                    return;
                }
            }
        }

    }

    public bool IsNonGroundObjectDetected()
    {
        //return true if we detected something that isn't the ground
        return _detectedObject != null && !_isGroundDetected;
    }

    public bool IsGroundDetected()
    {
        return _isGroundDetected;
    }

    public Vector3 GetGroundDetectionPoint()
    {
        return _detectedGroundPosition;
    }

    public bool IsAnythingDetected()
    {
        return _detectedObject != null;
    }

    public GameObject GetDetectedObject()
    {
        return _detectedObject;
    }


    //Debugging





}
