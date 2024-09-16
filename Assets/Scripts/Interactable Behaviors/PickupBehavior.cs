using UnityEngine;

public class PickupBehavior : MonoBehaviour, ITargetable
{
    //Declarations
    [SerializeField] private int _nutrition = 100;
    [SerializeField] private bool _isBeingCarried = false;
    [SerializeField] private bool _isReadyToBePickedUp = true;
    [SerializeField] private float _pickupCooldown = 1.5f;
    [SerializeField] private Rigidbody _rb;

    //Monos




    //Internals
    private void DisablePhysics()
    {
            _rb.isKinematic = true;
    }

    private void EnablePhysics()
    {
            _rb.isKinematic = false;
    }

    private void ReadyPickup()
    {
        _isReadyToBePickedUp = true;
    }




    //Externals
    public int Nutrition()
    {
        return _nutrition;
    }

    public int GetBehaviorID()
    {
        return GetInstanceID();
    }

    public InteractableType GetInteractableType()
    {
        return InteractableType.Pickup;
    }

    public Faction GetFaction()
    {
        return Faction.Neutral;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    

    public void SetPickupState(bool newState)
    {
        _isBeingCarried = newState;

        //make sure gravity is disabled when being carried
        if (_isBeingCarried)
            DisablePhysics();

        //make sure to enable gravity when being dropped
        else if (!_isBeingCarried)
        {
            EnablePhysics();

            //cooldown the pickup, so it doesn't get juggled each frame by ai
            _isReadyToBePickedUp = false;
            Invoke(nameof(ReadyPickup),_pickupCooldown);
        }
            
    }

    public bool IsPickedUp() { return _isBeingCarried; }

    public bool IsReadyForPickup()
    {
        return _isReadyToBePickedUp;
    }

    public void TakeDamage(ITargetable aggressor, int damage)
    {
        return;
    }

    public bool IsDead()
    {
        return false;
    }

    public int GetHealth()
    {
        return -1;
    }
}
