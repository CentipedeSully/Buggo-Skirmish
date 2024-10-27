using UnityEngine;
using System.Collections.Generic;




public class ModelRagdollController : MonoBehaviour
{
    //Declarations
    [SerializeField] private Rigidbody _modelRootRb;
    [SerializeField] private List<Rigidbody> _allModelRigidbodies;
    [SerializeField] private bool _isRagdollEnabled = false;



    //Monobehaviours
    private void Start()
    {
        SetRagdoll(false);
    }



    //Internals





    //Externals
    public void ConnectControllerRbToModel(Rigidbody controllerRb) 
    { 
        if (controllerRb != null)
            _modelRootRb.GetComponent<CharacterJoint>().connectedBody = controllerRb;
    }

    public void SetRagdoll(bool newState)
    {
        _isRagdollEnabled = newState;
            
        foreach (Rigidbody rb in _allModelRigidbodies)
        {
            rb.isKinematic = !_isRagdollEnabled;
            rb.detectCollisions = _isRagdollEnabled;
        }
    }





}
