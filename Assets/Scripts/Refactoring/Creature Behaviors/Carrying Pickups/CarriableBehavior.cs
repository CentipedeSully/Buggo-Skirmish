using UnityEngine;



public interface ICarriable
{
    GameObject GetGameObject();
    bool IsBeingCarried();
    bool IsReadyToBePickedUp();
    void SetCarryState(bool state);

    void EnableCarry(bool state);
    bool IsCarriableEnabled();
}


public class CarriableBehavior: MonoBehaviour, ICarriable
{
    //Declarations
    [SerializeField] protected bool _isCarriableEnabled = true;
    [SerializeField] protected bool _isBeingCarried = false;
    [SerializeField] protected bool _isReadyToBePickedUp = true;
    [SerializeField] protected float _pickupCooldown = 1.5f;
    protected Rigidbody _rb;



    //Monobehaviours
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }




    //Internals
    private void ReadyPickup()
    {
        _isReadyToBePickedUp = true;
    }

    protected virtual void ApplyOtherActionsOnPickup() { }
    protected virtual void ApplyOtherActionsOnDrop() { }


    //Externals
    public void SetCarryState(bool newState)
    {
        if (newState != _isBeingCarried && _isCarriableEnabled)
        {
            _isBeingCarried = newState;

            
            if (_isBeingCarried)
            {
                //make sure other physical influences are disabled when being carried
                _rb.isKinematic = true;

                ApplyOtherActionsOnPickup();
            }
                

            
            else if (!_isBeingCarried)
            {
                //make sure other physical influences are enabled when being dropped
                _rb.isKinematic = false;

                //cooldown the pickup, so it doesn't get juggled each frame by ai
                _isReadyToBePickedUp = false;
                Invoke(nameof(ReadyPickup), _pickupCooldown);

                ApplyOtherActionsOnDrop();
            }
        }
        

    }

    public GameObject GetGameObject() { return gameObject; }

    public bool IsBeingCarried() { return _isBeingCarried; }

    public bool IsReadyToBePickedUp() {  return _isReadyToBePickedUp && _isCarriableEnabled; }

    public void EnableCarry(bool state) { _isCarriableEnabled = state; }

    public bool IsCarriableEnabled() {  return _isCarriableEnabled; }
}
