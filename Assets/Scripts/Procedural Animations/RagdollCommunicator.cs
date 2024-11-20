using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class RagdollCommunicator : MonoBehaviour
{
    //Declarations
    private RagdollController _ragdollController;
    private Rigidbody _controllerRb;
    private NavMeshAgent _navAgent;


    //Monobehaviours
    private void Awake()
    {
        _ragdollController = GetComponentInChildren<RagdollController>();
        _controllerRb = GetComponent<Rigidbody>();
        _navAgent = GetComponent<NavMeshAgent>();

        _ragdollController.SetParentRb(_controllerRb);
        
    }




    //Internals





    //Externals
    [Button]
    public void ToggleRagdoll(bool state)
    {
        if (_ragdollController != null)
        {
            //disable any navAgent and unlock any movementRb
            _navAgent.enabled = !state;

            _ragdollController.SetRagdoll(state);
        }
            
    }

}

