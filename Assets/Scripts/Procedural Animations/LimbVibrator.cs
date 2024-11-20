using Sirenix.OdinInspector;
using UnityEngine;

public class LimbVibrator : MonoBehaviour
{
    //Declarations
    [SerializeField] private Vector3 _origin;
    [SerializeField] private float _magnitude = .2f;
    [SerializeField] private float _duration = .05f;
    private float _currentTime;
    [SerializeField] private float _frequency = .01f;
    [ReadOnly]
    [SerializeField] private bool _isVibrating = false;

    public Vector3 Origin { get => _origin; set => _origin = value; }
    public float Magnitude { get => _magnitude; set => _magnitude = value; }
    public float Duration { get => _duration; set => _duration = value; }




    //Monobehaviours
    private void Awake()
    {
        _origin = transform.localPosition;
    }

    private void Update()
    {
        if (_isVibrating)
            TickVibrate();
    }


    //Internals
    private Vector3 RandomizePosition(float xAbsRange, float yAbsRange, float zAbsRange)
    {
        float x = Random.Range(-xAbsRange, xAbsRange);
        float y = Random.Range(-yAbsRange, yAbsRange);
        float z = Random.Range(-zAbsRange, zAbsRange);

        return new Vector3(x, y, z);
    }

    private void TickVibrate()
    {
        _currentTime += Time.deltaTime;
        
        if (_currentTime >= _duration)
        {
            _isVibrating = false;
            _currentTime = 0;
            CancelInvoke(nameof(Reposition));
            transform.localPosition = _origin;
        }
    }

    private void Reposition()
    {
        transform.localPosition = RandomizePosition(_magnitude,_magnitude,_magnitude) + _origin;
    }





    //Externals
    public bool IsVibrating() {  return _isVibrating; }

    [Button]
    public void Vibrate()
    {
        //reset the time if we're already vibrating
        if (_isVibrating) 
            _currentTime = 0;

        //else setup the utils
        else
        {
            _isVibrating = true;
            InvokeRepeating(nameof(Reposition), 0, _frequency);
        }
    }



}
