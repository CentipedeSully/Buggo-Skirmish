using System.Collections;
using UnityEngine;
using System.Collections.Generic;




public class NestBehavior : MonoBehaviour, ITargetable
{
    //Declarations
    [SerializeField] private int _currentNutrition;
    [SerializeField] private int _eggCost = 100;
    [SerializeField] private float _hatchTime = 3f;
    [SerializeField] private int _eggsIncubating = 0;
    [SerializeField] private GameObject _minionPrefab;
    [SerializeField] private Transform _entitiesContainer;
    [SerializeField] private Faction _faction = Faction.Ally;
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private Transform _minionSpawnPosition;




    //Monobehaviours
    private void OnTriggerStay(Collider other)
    {
        ITargetable behavior = other.GetComponent<ITargetable>();

        if (behavior != null)
        {
            if (behavior.GetInteractableType() == InteractableType.Pickup)
            {
                //wait for the pickup to be dropped!
                if (behavior.IsPickedUp() == false)
                {
                    //increase the nest's nutrition value
                    _currentNutrition += behavior.Nutrition();

                    //destroy the pickup
                    Destroy(other.gameObject);

                    //can we create an egg?
                    if (_currentNutrition >= _eggCost)
                    {

                        int incubations = _currentNutrition / _eggCost;

                        //Begin Incubating as many eggs as possible!
                        while (incubations > 0)
                        {
                            _currentNutrition -= _eggCost;
                            IncubateEgg();
                            incubations--;
                        }
                    }
                }
            }
        }
    }





    //Internals
    private void IncubateEgg()
    {
        _eggsIncubating++;
        Invoke(nameof(HatchMinion), _hatchTime);
    }

    private void HatchMinion()
    {
        //decrement the number of eggs being incubated
        _eggsIncubating--;

        //SpawnMinion
        GameObject newMinionObject =Instantiate(_minionPrefab, _minionSpawnPosition.position, Quaternion.identity,_entitiesContainer);

        //setup the minion's utils
        newMinionObject.GetComponent<AiBehavior>().SetNest(gameObject);
    }






    //Externals
    public int GetBehaviorID()
    {
        return GetInstanceID();
    }

    public InteractableType GetInteractableType()
    {
        return InteractableType.Nest;
    }

    public Faction GetFaction()
    {
        return _faction;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public int Nutrition()
    {
        return 0;
    }

    public void SetPickupState(bool state)
    {
        //ignore
    }

    public bool IsPickedUp()
    {
        return false;
    }
}
