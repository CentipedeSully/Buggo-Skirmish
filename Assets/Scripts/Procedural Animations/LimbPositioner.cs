using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.Animations.Rigging;
using System.Collections;






public class LimbPositioner : MonoBehaviour
{
    //Declarations
    [SerializeField] private MultiPositionConstraint _constraintRef;
    [ReadOnly]
    [SerializeField] private Vector3 _defaultLocalPosition;
    [SerializeField] private float _resetDuration = .2f;
    [ReadOnly]
    [SerializeField] bool _isTransitioning;

    private Vector3 _startLocalPosition;
    private Vector3 _targetLocalPosition;
    private float _transitionDuration;
    private float _currentDuration;
    


    //Monobehaviours
    private void Awake()
    {
        _defaultLocalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (_isTransitioning)
            PerformTransition();
    }


    //Internals
    private void PerformTransition()
    {
        _currentDuration += Time.deltaTime;
        transform.localPosition = Vector3.Lerp(_startLocalPosition, _targetLocalPosition, _currentDuration / _transitionDuration);

        if (_currentDuration >= _transitionDuration)
        {
            _currentDuration = 0;
            _isTransitioning = false;
        }
    }


    


    //Externals
    public Vector3 GetCurrentPosition() { return transform.localPosition; }

    [Button]
    public void SetPosition(Vector3 newLocalPosition, float duration)
    {
        
        //reset time (if we're mid transition)
        _currentDuration = 0;
            
        //setup transition utils
        _startLocalPosition = transform.localPosition;
        _targetLocalPosition = newLocalPosition;
        _transitionDuration = duration;

        //enable the constraint
        _constraintRef.weight = 1.0f;

        //perform transition on Update until completed
        _isTransitioning = true;
    }

    [Button]
    public void ResetPosition() 
    {
        //Move back to the default position
        SetPosition(_defaultLocalPosition, _resetDuration);
    }


}
