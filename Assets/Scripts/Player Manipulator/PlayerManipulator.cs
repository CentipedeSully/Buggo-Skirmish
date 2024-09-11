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




    //Internals

    

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
                break;
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
                    break;
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
                    break;
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
