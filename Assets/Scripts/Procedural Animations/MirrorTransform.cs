using UnityEngine;

public class MirrorTransform : MonoBehaviour
{
    //Declarations
    [SerializeField] private Transform _source;
    [SerializeField] private bool _isMirroringActive = true;



    //Monobehaviours
    private void Update()
    {
        if (_isMirroringActive && _source != null)
            MirrorSource();
    }




    //Internals
    private void MirrorSource()
    {
        transform.position = _source.position;
        transform.rotation = _source.rotation;
    }



    //Externals
    public void EnableMirroring() { _isMirroringActive=true; }

    public void DisableMirroring() { _isMirroringActive = false; }



}
