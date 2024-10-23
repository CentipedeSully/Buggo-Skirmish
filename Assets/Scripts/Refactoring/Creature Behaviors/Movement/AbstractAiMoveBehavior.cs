using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;




public interface IAiMoveBehavior
{
    bool IsMoving();
    void ClearCurrentMovement();
    void MoveToLocation(Vector3 position);
    void ApproachTarget(Transform target);

}



public abstract class AbstractAiMoveBehavior : MonoBehaviour, IAiMoveBehavior, ICreatureBehavior
{
    //Declarations
    [Header("Default Move Utils")]
    [SerializeField] protected float _stopRange = .25f;
    [SerializeField] protected bool _drawStopRange = false;
    [SerializeField] protected Color _stopRangeColor = Color.yellow;
    [SerializeField] protected Vector3 _currentDestination;
    [SerializeField] protected Transform _currentTarget;
    [SerializeField] protected bool _isMoving = false;


    //Monobehaviours
    private void Update()
    {
        if (_isMoving)
            UpdateMoveUtils();
    }

    private void OnDrawGizmosSelected()
    {
        DrawStopRange();
    }



    //Internals
    protected virtual bool IsDestinationCloseEnough(Vector3 destination)
    {
       return Vector3.Distance(destination,transform.position) <= _stopRange;
    }
    protected virtual void UpdateMoveUtils()
    {
        //are we pursuing a target? Then use the target's positional data
        if (_currentTarget != null)
        {
            if (IsDestinationCloseEnough(_currentTarget.transform.position))
                ClearCurrentMovement();

            else
                UpdateOtherTargetBasedMoveUtils();
        }

        //else, just use our cached destination
        else
        {
            if (IsDestinationCloseEnough(_currentDestination))
                ClearCurrentMovement();

            else
                UpdateOtherLocationBasedMoveUtils();
        }
        
    }

    protected abstract void UpdateOtherTargetBasedMoveUtils();
    protected abstract void UpdateOtherLocationBasedMoveUtils();
    protected abstract void ClearOtherMoveUtils();
    protected abstract void SetupOtherTargetMoveUtils();
    protected abstract void SetupOtherDestinationMoveUtils();
    protected virtual void DrawStopRange()
    {
        if (_drawStopRange)
        {
            Gizmos.color = _stopRangeColor;
            Gizmos.DrawWireSphere(transform.position, _stopRange);
        }
    }


    //Externals
    public virtual void ApproachTarget(Transform target)
    {
        //ignore null and duplicate targets
        if (target != null && _currentTarget != target)
        {
            if (_isMoving)
                ClearCurrentMovement();

            _isMoving = true;
            _currentTarget = target;
            SetupOtherTargetMoveUtils();
        }
    }

    public virtual void ClearCurrentMovement()
    {
        if (_isMoving)
        {
            _isMoving = false;
            ClearOtherMoveUtils();
            _currentDestination = transform.position;
            _currentTarget = null;
        }
    }

    public bool IsMoving() { return _isMoving; }

    public virtual void MoveToLocation(Vector3 position)
    {
        //ignore locations that're already really close
        if (!IsDestinationCloseEnough(position))
        {
            if (_isMoving)
                ClearCurrentMovement();

            _isMoving = true;
            _currentDestination = position;
            SetupOtherDestinationMoveUtils();
        }
    }

    public abstract void ReadCreatureData(CreatureData data);
}
