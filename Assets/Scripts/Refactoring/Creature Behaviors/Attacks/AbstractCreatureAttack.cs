using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Rendering.Universal;





[Serializable]
public struct AtkAnimData
{
    public string animName;
    public AtkState playState;
    public LimbPositioner limbPositioner;
    public Vector3 _endPosition;
}


public abstract class AbstractCreatureAttack : AbstractAttackBehavior, ICreatureBehavior
{
    //Declarations
    [TabGroup("Creature Attack Settings", "Animations")]
    [SerializeField] protected List<AtkAnimData> _animationData = new List<AtkAnimData>();
    [TabGroup("Creature Attack Settings", "Animations")]
    [ReadOnly] protected List<AtkAnimData> _atkPrepAnimations = new List<AtkAnimData>();
    [TabGroup("Creature Attack Settings", "Animations")]
    [ReadOnly] protected List<AtkAnimData> _atkCastAnimations = new List<AtkAnimData>();
    [TabGroup("Creature Attack Settings", "Animations")]
    [ReadOnly] protected List<AtkAnimData> _atkRecoveryAnimations = new List<AtkAnimData>();




    //Monobehaviours
    private void Awake()
    {
        SortAnimationData();
    }




    //Internals
    protected void SortAnimationData()
    {
        foreach (AtkAnimData data in _animationData)
        {
            switch (data.playState)
            {
                case AtkState.preparingAtk:
                    _atkPrepAnimations.Add(data);
                    break;

                case AtkState.castingAtk:
                    _atkCastAnimations.Add(data);
                    break;

                case AtkState.recoveringAtk:
                    _atkRecoveryAnimations.Add(data);
                    break;
            }
        }
    }

    protected void AnimateLimbs(AtkState newState)
    {
        switch (newState)
        {
            case AtkState.notAttacking:
                break;


            case AtkState.preparingAtk:
                foreach (AtkAnimData animData in _atkPrepAnimations)
                    animData.limbPositioner.SetPosition(animData._endPosition, _atkPreparationDuration);

                break;


            case AtkState.castingAtk:
                foreach (AtkAnimData animData in _atkCastAnimations)
                    animData.limbPositioner.SetPosition(animData._endPosition, _atkCastDuration);

                break;


            case AtkState.recoveringAtk:
                foreach (AtkAnimData animData in _atkRecoveryAnimations)
                    animData.limbPositioner.SetPosition(animData._endPosition, _atkRecoveryDuration);

                break;


            default:
                break;

        }
        
    }



    //Externals
    public void InterruptBehavior() { InterruptAttack(); }
    public void ReadCreatureData(CreatureData data)
    {
        _damageInfo.Damage = data.GetBaseDamage();
    }
}
