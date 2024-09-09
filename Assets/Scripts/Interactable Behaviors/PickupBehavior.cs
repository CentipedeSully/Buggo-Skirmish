using UnityEngine;

public class PickupBehavior : MonoBehaviour, ITargetable
{
    //Declarations
    [SerializeField] private int _nutrition = 100;


    //Monos






    //Internals







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

    public void TriggerGravity()
    {
        GetComponent<Rigidbody>().useGravity = true;
    }




}
