using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Animations.Rigging;




public class ModelRagdollController : MonoBehaviour
{
    //Declarations
    [SerializeField] private Rigidbody _modelRootRb;
    [SerializeField] private List<Rigidbody> _allModelRigidbodies;
    [SerializeField] private List<MultiPositionConstraint> _positionConstraints= new List<MultiPositionConstraint>();
    [ReadOnly]
    [SerializeField] private bool _isRagdollEnabled = false;
    private FootDisplacementManager _footDisplacementManager;


    //Monobehaviours
    private void Awake()
    {
        _footDisplacementManager = GetComponent<FootDisplacementManager>();
    }

    private void Start()
    {
        SetRagdoll(false);
    }



    //Internals
    private void SetProceduralConstraintWeights(int newWeight)
    {
        foreach (MultiPositionConstraint posConstraint in _positionConstraints)
            posConstraint.weight = newWeight;
    }




    //Externals
    public void ConnectControllerRbToModel(Rigidbody controllerRb) 
    { 
        if (controllerRb != null)
            _modelRootRb.GetComponent<ConfigurableJoint>().connectedBody = controllerRb;
    }

    public void SetRagdoll(bool newState)
    {
        _isRagdollEnabled = newState;
            
        foreach (Rigidbody rb in _allModelRigidbodies)
        {
            if (_isRagdollEnabled)
                rb.constraints = RigidbodyConstraints.None;
            else rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        if (_isRagdollEnabled)
        {
            SetProceduralConstraintWeights(0);
            _footDisplacementManager.EnabledDisplacement(false);
        }
            
        else
        {
            SetProceduralConstraintWeights(1);
            _footDisplacementManager.EnabledDisplacement(true);
        }
    }

    public bool IsRagdollEnabled() { return _isRagdollEnabled; }




}
