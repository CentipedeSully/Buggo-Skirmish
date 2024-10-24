using UnityEngine;
using System.Collections;
using Sirenix;
using Sirenix.OdinInspector;

public interface IAttackBehaviour
{
    void AttemptAttack();
    void InterruptAttack();
    void SetTarget(GameObject newTarget);
    GameObject GetCurrentTarget();
    bool IsAttacking();
}



public class BiteAttack : MonoBehaviour, IAttackBehaviour, ICreatureBehavior
{
    //Declarations
    [TabGroup("Bite Attack", "Setup")]
    [SerializeField] private LayerMask _attackLayerMask;
    [TabGroup("Bite Attack", "Setup")]
    [SerializeField] private Transform _atkCastOffset;

    [TabGroup("Bite Attack", "Setup")]
    [SerializeField] private float _atkCastRadius = 2;
    [TabGroup("Bite Attack", "Setup")]
    [SerializeField] private Color _atkRangeGizmoColor = Color.red;
    [TabGroup("Bite Attack", "Setup")]
    [SerializeField] private bool _showAtkRangeGizmo = false;
    [TabGroup("Bite Attack", "Setup")]
    [SerializeField] private float _atkAnimDuration;
    [TabGroup("Bite Attack", "Setup")]
    [SerializeField][Range(0, 1)] private float _atkCastRelativeStartTime;
    [TabGroup("Bite Attack", "Setup")]
    [SerializeField] private float _atkCastDuration = .2f;

    [TabGroup("Bite Attack", "Setup")]
    [Tooltip("How much tolerance does this minion have when turning to face a target. 0 means no tolerance. 180 means 'I don't need to see you to hit you'")]
    [SerializeField][Range(0, 180)] private float _atkAlignmentTolerance = 10;
    [TabGroup("Bite Attack", "Setup")]
    [SerializeField] private bool _showAtkAlignmentRays = false;


    [TabGroup("Bite Attack", "Info")]
    [SerializeField] private GameObject _currentTargetObject;
    [TabGroup("Bite Attack", "Info")]
    [SerializeField] private int _currentTargetID;
    [TabGroup("Bite Attack", "Info")]
    [SerializeField] private bool _isAtkReady = true;
    [TabGroup("Bite Attack", "Info")]
    [SerializeField] private bool _isAttacking = false;
    [TabGroup("Bite Attack", "Info")]
    [SerializeField] private int _damage;
    [TabGroup("Bite Attack", "Info")]
    [SerializeField] private float _atkCooldown = .5f;

    
    private float _alignmentRotationSpeed = 90;
    private IEnumerator _atkSequence = null;
    private float _atkCastStartTime;
    private float _remainingAtkTime;
    private bool _isCastingAttack = false;
    private bool _isAtkCoolingDown = false;



    //Monobehaviours
    private void OnDrawGizmosSelected()
    {
        DrawRangeGizmos();
    }




    //Internals
    private void DrawRangeGizmos()
    {
        if (_showAtkRangeGizmo)
        {
            Gizmos.color = _atkRangeGizmoColor;
            Gizmos.DrawWireSphere(_atkCastOffset.position, _atkCastRadius);
        }
    }

    private void AlignAttack()
    {
        //is our target valid (and we aren't already committed to an attack)
        if (_currentTargetObject != null && !_isAttacking)
        {
            //Calculate the direction towards the target
            Vector3 targetDirection = _currentTargetObject.transform.position - transform.position;

            //ignore the direction's y, In cases where enemies are too short or too high
            targetDirection = new Vector3(targetDirection.x, 0, targetDirection.z);

            //Normalize the direction
            Vector3 normalizedDirectionTowardsTarget = targetDirection.normalized;

            //calculate our forwards direction (in world space)
            Vector3 forwardsDirection = transform.TransformVector(Vector3.forward);

            //Draw the alignment vectors used in our calculations, if needed
            if (_showAtkAlignmentRays)
            {
                Debug.DrawRay(transform.position, forwardsDirection * 5, Color.red);
                Debug.DrawRay(transform.position, normalizedDirectionTowardsTarget * 5, Color.yellow);
            }

            //calculate the angle between our forwards and the normalizedTargetDirection
            float angleDifference = Vector3.SignedAngle(forwardsDirection, normalizedDirectionTowardsTarget, Vector3.up);

            //Log the angular difference, if needed (Use this in case the minion approaches a target but doesn't attack it)
            //Debug.Log($"angle difference: {angleDifference}");

            //is the target within a tolerable alignment
            if (-_atkAlignmentTolerance <= angleDifference && angleDifference <= _atkAlignmentTolerance)
                EnterAttack();

            //we aren't properly aligned to the target
            else
            {
                //calculate the additive rotation. we're rotating on the y axis in the direction of the angular difference's sign
                Vector3 additiveRotation = Mathf.Sign(angleDifference) * Vector3.up * _alignmentRotationSpeed * Time.deltaTime;

                //Apply the rotation
                transform.eulerAngles += additiveRotation;
            }
        }
    }

    private void ReadyAtk()
    {
        _isAtkCoolingDown = false;
        _isAtkReady = true;
    }

    private void CooldownAtk()
    {
        if (!_isAtkCoolingDown)
        {
            _isAtkCoolingDown = true;
            Invoke(nameof(ReadyAtk), _atkCooldown);
        }
    }

    private void CastAttack()
    {
        //if we're in the cast state
        if (_isCastingAttack)
        {
            //Did our target vanish?
            if (_currentTargetObject == null)
            {
                //cancel any timers that're counting the duration of our cast
                CancelInvoke(nameof(EndAttackCast));

                //end the attack cast now
                EndAttackCast();
                return;
            }

            //Cast over the detection area
            Collider[] detections = Physics.OverlapSphere(_atkCastOffset.position, _atkCastRadius, _attackLayerMask);

            //Attempt to find our target amongst any detections
            foreach (Collider detection in detections)
            {
                //has our target been detected?
                if (detection.GetComponent<IEntityID>().GetEntityID() == _currentTargetID)
                {
                    //Damage the target
                    ///Idea
                    ///Have every collider that's attackable have a HealthBehavior that references an EntityID

                    //cancel any timers that're counting the duration of this cast
                    CancelInvoke(nameof(EndAttackCast));

                    //End the cast, the target has been hit
                    EndAttackCast();
                    return;
                }
            }
        }
    }

    private void EndAttackCast()
    {
        _isCastingAttack = false;
    }

    private void EnterAttack()
    {
        //are we NOT already attacking?
        if (_atkSequence == null && _isAtkReady)
        {
            //Set the atk reference
            _atkSequence = ManageAttackSequence();

            //calculate the relative atkCast time
            _atkCastStartTime = _atkAnimDuration * _atkCastRelativeStartTime;

            //calculate the remainder atk time
            _remainingAtkTime = _atkAnimDuration - _atkCastStartTime;

            //start the sequence
            StartCoroutine(_atkSequence);
        }
    }

    private void CancelAttack()
    {
        //are we currently in our attack sequence?
        if (_atkSequence != null)
        {
            //stop the sequence timer
            StopCoroutine(_atkSequence);

            //Clear the reference
            _atkSequence = null;

            //leave the atk state
            _isAttacking = false;

            //stop casting any attacks, if they exist
            CancelInvoke(nameof(CancelAttack));
            EndAttackCast();

            //update the animator that we aren't attacking anymore
            //_bodyAnimator.SetBool("isAttacking", false);

            //Cooldown the attack, if it isn't cooling down already
            CooldownAtk();
        }
    }

    private IEnumerator ManageAttackSequence()
    {
        //enter the atk state
        _isAttacking = true;

        //reest the atk ready state
        _isAtkReady = false;

        //enter the proper animation
        //_bodyAnimator.SetBool("isAttacking", true);

        //wait for the atkCast time
        yield return new WaitForSeconds(_atkCastStartTime);

        //Cast the atk
        _isCastingAttack = true;
        Invoke(nameof(EndAttackCast), _atkCastDuration);

        //play the attack sound
        //_audioController.PlayAttackingSound();

        //wait for the remainder of the sequence
        yield return new WaitForSeconds(_remainingAtkTime);

        //exit the atk state
        _isAttacking = false;

        //clear our reference
        _atkSequence = null;

        //Exit the attack animation
        //_bodyAnimator.SetBool("isAttacking", false);

        //Cooldown the atk
        CooldownAtk();
    }



    //Externals
    public void ReadCreatureData(CreatureData data)
    {
        _damage = data.GetBaseDamage();
        _alignmentRotationSpeed = data.GetBaseTurnSpeed();
        _atkCooldown = data.GetBaseAtkCooldown();
    }

    [TabGroup("Bite Attack", "Debug")]
    [Button]
    public void SetTarget(GameObject newTarget)
    {
        //ignore blank targets
        if (newTarget != null)
        {

            IEntityController controller = newTarget.GetComponent<IEntityController>();

            //only target objects with controllers. Do this to get their controllerID. Used for atttack casting.
            if (controller != null)
            {
                if (_isAttacking)
                    InterruptAttack();

                _currentTargetObject = newTarget;
                _currentTargetID = controller.GetEntityID();
            }
        }
        
    }

    [TabGroup("Bite Attack", "Debug")]
    [Button]
    public void AttemptAttack() { EnterAttack(); }

    [TabGroup("Bite Attack", "Debug")]
    [Button]
    public void InterruptAttack() { CancelAttack(); }



    public GameObject GetCurrentTarget() {return _currentTargetObject;}

    public bool IsAttacking() { return _isAttacking;}

    public void InterruptBehavior()
    {
        InterruptAttack();
    }
}
