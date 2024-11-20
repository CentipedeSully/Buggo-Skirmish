using Sirenix.OdinInspector;
using UnityEngine;


public interface ICarryBehavior
{
    GameObject GetGameObject();
    void CarryObject(ICarriable target);
    void DropItem();
    GameObject GetCarriedObject();
    bool IsCarryingObject();
}

public class CarrySingleBehavior : MonoBehaviour, ICreatureBehavior, ICarryBehavior
{
    //Declarations
    [SerializeField] private Transform _carryPosition;
    [SerializeField] private GameObject _carriedObject;
    [ReadOnly]
    [SerializeField] private bool _isCarryingObject = false;

    private CreatureType _creatureType = CreatureType.unset;
    private bool _isLerping = false;
    private float _lerpDuration = .2f;
    private float _currentLerpTime = 0;
    private Vector3 _startPosition;
    private CommunicateToAnimators _animatorCommunicator;






    //Monobehaviours
    private void Awake()
    {
        _animatorCommunicator = GetComponent<CommunicateToAnimators>();
    }
    private void Update()
    {
        LerpObjectOnPickup();
    }




    //Internals
    private void LerpObjectOnPickup()
    {
        if (_isLerping)
        {
            _currentLerpTime += Time.deltaTime;
            _carriedObject.transform.position = Vector3.Lerp(_startPosition, _carryPosition.position, _currentLerpTime / _lerpDuration);

            if (_currentLerpTime >= _lerpDuration)
            {
                _isLerping = false;
                _currentLerpTime = 0;
            }
        }
    }



    //Externals
    [BoxGroup("Debug")]
    [Button]
    public void DropItem()
    {
        if (_isCarryingObject)
        {
            //cancel any lerping being performed
            if (_isLerping)
            {
                _isLerping = false;
                _currentLerpTime = 0;
            }

            //reset our own carrying state
            _isCarryingObject = false;

            //unbind the object from our carryPosition
            _carriedObject.transform.SetParent(null);

            //Update the carried-object's state
            _carriedObject.GetComponent<ICarriable>().SetCarryState(false);

            //forget the object
            _carriedObject = null;

            //Update animators
            if (_animatorCommunicator != null)
            {
                _animatorCommunicator.SetPickupAnimation(false, _creatureType);
                _animatorCommunicator.SetMandibles(JawState.open);
            }
            
        }
        
    }

    public GameObject GetCarriedObject() { return _carriedObject; }
    public GameObject GetGameObject() { return gameObject; }
    public void InterruptBehavior() { DropItem(); }
    public bool IsCarryingObject() { return _isCarryingObject; }

    [BoxGroup("Debug")]
    [Button]
    public void CarryObject(ICarriable target)
    {
        //ignore blank pickups
        if (target != null)
        {
            //make sure taget is valid before picking it up
            if (!target.IsBeingCarried() && target.IsReadyToBePickedUp())
            {
                //Drop our current pickup if we're already carrying
                if (_isCarryingObject)
                    DropItem();

                //update our own carryState
                _isCarryingObject = true;

                //update the carriableObject
                target.SetCarryState(true);

                //Set as our new carried item
                _carriedObject = target.GetGameObject();

                //reparent the newItem
                _carriedObject.transform.SetParent(_carryPosition, true);

                //begin smoothly transitioning the item into place overtime
                _startPosition = _carriedObject.transform.position;
                _isLerping = true;

                //update animators
                if (_animatorCommunicator != null)
                {
                    _animatorCommunicator.SetPickupAnimation(true,_creatureType);
                    _animatorCommunicator.SetMandibles(JawState.closed);
                }

            }

        }
    }

    public void ReadCreatureData(CreatureData data)
    {
        _creatureType = data.GetCreatureType();
    }
}
