using UnityEngine;
using Sirenix;
using Sirenix.OdinInspector;

public class HowToBuildAModel : MonoBehaviour
{
    [InfoBox(
        "Collisions:\nAny object with a collider that needs to take damage needs an EntityID component attatched. " +
        "Also be sure to reference all EntityIDComponents from the core EntityController that'll parent this entire model," +
        " if many different colliders exist.\n\nProcedural Animations:\nAdd any procedural-animation scripts its respective limb," +
        " and then be sure to reference all of these scripts from the Parent object's AnimatorCommunicator, if necessary.\n\nRagDolling:\n" +
        "First apply Character joints to every major bodypart and connector object. Link the " +
        "joints to their respective parent rigibodies (charJoints apply rb's automatically). Then add a ragdoll controller to the Model's parent " +
        "container and then specify the rootRb and add all rbs of the model to the controller. " +
        "Lastly, apply a RagdollCommunicator to the model's parent Controller object. The communicator will do everything else."
        )]
    public bool _defaultAttribute;
        
}
