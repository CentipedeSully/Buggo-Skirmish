using UnityEngine;
using UnityEngine.Animations.Rigging;

public class SetLookAtParent: MonoBehaviour
{
    [SerializeField] private Transform _desiredParent;

    private void Awake()
    {
        MultiParentConstraint mpContraint = GetComponent<MultiParentConstraint>();

        if (mpContraint != null)
        {
            WeightedTransform source = new WeightedTransform(_desiredParent, 1);

            WeightedTransformArray a = mpContraint.data.sourceObjects;

            a.Add(source);

            mpContraint.data.sourceObjects = a;
        }
    }

}
