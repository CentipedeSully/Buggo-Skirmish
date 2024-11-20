using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Sirenix.OdinInspector;

public class RagdollController : MonoBehaviour
{
    //Declarations
    [BoxGroup("Setup")]
    [SerializeField] private Rigidbody _parentRb;
    [BoxGroup("Setup")]
    [SerializeField] private ConfigurableJoint _rootPhysicsJoint;
    [BoxGroup("Setup")]
    [SerializeField] private GameObject _model;
    [BoxGroup("Setup")]
    [SerializeField] private GameObject _rigObject;
    [BoxGroup("Setup")]
    [SerializeField] private Animator _rigAnimator;
    [BoxGroup("Setup")]
    [SerializeField] private GameObject _damageableCollidersParent;

    [TabGroup("Info", "Status")]
    [ReadOnly]
    [SerializeField] private bool _isRagdollEnabled = false;

    [TabGroup("Info", "Model Rbs")]
    [ReadOnly]
    [SerializeField] private Rigidbody[] _modelRbs;

    [TabGroup("Info", "Rig")]
    [ReadOnly]
    [SerializeField] private Rig _rig;
    

    [TabGroup("Info", "Dmg Colliders")]
    [ReadOnly]
    [SerializeField] private Collider[] _damageableColliders;



    //Monobehaviours
    private void Awake()
    {
        InitializeReferences();
    }

    private void Start()
    {
        SetRagdoll(false);
    }



    //Internals
    private void InitializeReferences()
    {
        SetParentRb(_parentRb);

        if (_model != null)
            _modelRbs = _model.GetComponentsInChildren<Rigidbody>();

        if (_rigObject != null)
            _rig = _rigObject.GetComponent<Rig>();

        if (_damageableCollidersParent != null)
            _damageableColliders = _damageableCollidersParent.GetComponentsInChildren<Collider>();
    }



    private void SetRigWeight(int newWeight)
    {
        if (_rig != null)
            _rig.weight = newWeight;
    }




    //Externals
    [BoxGroup("Debug")]
    [Button]
    public void SetParentRb(Rigidbody newParent)
    {
        if (newParent != null)
        {
            _parentRb = newParent;

            if (_rootPhysicsJoint != null)
                _rootPhysicsJoint.connectedBody = _parentRb;
        }
    }

    [BoxGroup("Debug")]
    [Button]
    public void SetRagdoll(bool newState)
    {
        _isRagdollEnabled = newState;

        foreach (Rigidbody rb in _modelRbs)
        {
            if (_isRagdollEnabled)
                rb.isKinematic = false;
            else rb.isKinematic = true;

            //Enabling Kinematics disables all physics interactions, including damage-casting
            //We use the mirrors to detect our damage collisions while the body is kinematic
        }

        
        if (_isRagdollEnabled)
        {
            //cut the relationship btwn the animationRig and the limbs
            SetRigWeight(0);

            //disable the damageable-Collider Mirrors
            _damageableCollidersParent.SetActive(false);

            //deactivate the animator
            _rigAnimator.enabled = false;
                
        }

        else
        {
            //restore the relationship btwn the animRig and the limbs
            SetRigWeight(1);

            //enable the damageable-Collider Mirrors
            _damageableCollidersParent.SetActive(true);

            //reactivate the animator
            _rigAnimator.enabled=true;
        }
            
    }

    public bool IsRagdollEnabled() { return _isRagdollEnabled; }
}
