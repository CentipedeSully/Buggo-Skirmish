using UnityEngine;

public class PickupBehavior : MonoBehaviour, ITargetable
{
    //Declarations
    [SerializeField] private int _nutrition = 100;
    [SerializeField] private bool _isBeingCarried = false;
    [SerializeField] private bool _isReadyToBePickedUp = true;
    [SerializeField] private float _pickupCooldown = .5f;
    [SerializeField] private bool _isGravityEnabled = false;
    [Tooltip("How long a pickup will wait before removing itself from gravity after being dropped")]
    [SerializeField] private float _gravityResetTime = 2f;
    private Rigidbody _rb;

    //Monos

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }




    //Internals
    private void DisableGravity()
    {
        if (_isGravityEnabled)
        {
            _rb.useGravity = false;
            _isGravityEnabled = false;

            CancelInvoke(nameof(DisableGravity));
        }
    }

    private void TemporarilyEnableGravity()
    {
        //is gravity off?
        if (!_isGravityEnabled)
        {
            _rb.useGravity = true;
            _isGravityEnabled = true;

            //start counting down before gravity is stopped
            Invoke(nameof(DisableGravity), _gravityResetTime);
        }
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
            DisableGravity();

        //make sure to enable gravity when being dropped
        else if (!_isBeingCarried)
        {
            TemporarilyEnableGravity();

            //cooldown the pickup, so it doen't get juggled each frame by ai
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
