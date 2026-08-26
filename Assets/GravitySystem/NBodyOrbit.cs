using UnityEngine;
using PurrNet;

// N-Body orbiters should not have a gravity source component, as they are meant to represent ships with minimal mass.
public class NBodyOrbit : NetworkIdentity
{
    [Header("N-Body Orbit Settings")]
    [SerializeField] private float _baseMass;
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private bool _giveInitialVelocity = true;
    [Header("Orbit Drawing")]
    [SerializeField] private bool _drawOrbit = false;
    [SerializeField] private float _timeStep = 0.02f;
    [SerializeField] private float _drawOrbitTime = 10f;
    [SerializeField] private Color _orbitColor = Color.green;
    private NetworkedTime _networkedTime;

    void Start()
    {
        if (_giveInitialVelocity)
            InitVelocity();

        _networkedTime = FindFirstObjectByType<NetworkedTime>();
    }

    private void FixedUpdate()
    {
        if (_drawOrbit)
        {
            DrawOrbitToTime(_networkedTime.ClientSimulationTime + _drawOrbitTime, _timeStep);
        }
        GravitySource[] sources = FindObjectsByType<GravitySource>(FindObjectsSortMode.None);
        
        foreach (var source in sources)
        {
            float m2 = source.mass;
            float r = Vector3.Distance(transform.position, source.transform.position);

            _velocity += (source.transform.position - transform.position).normalized * (GravityConstant.G * (_baseMass * m2) / (r * r)) * Time.fixedDeltaTime * _networkedTime.TimeScale; // simplified gravitational equation
        }

        transform.position += _velocity * Time.fixedDeltaTime * _networkedTime.TimeScale;
    }

    private void Update()
    {
        
    }

    private void InitVelocity()
    {
        GravitySource[] sources = FindObjectsByType<GravitySource>(FindObjectsSortMode.None);
        
        foreach (var source in sources)
        {
            float m2 = source.mass;
            float r = Vector3.Distance(transform.position, source.transform.position);
            transform.LookAt(source.transform);

            _velocity += transform.right * Mathf.Sqrt((GravityConstant.G * m2) / r);
        }
    }

    private void DrawOrbitToTime(float stopTime, float timeStep)
    {
        if (timeStep == 0f)
        {
            Debug.LogWarning("Time step must be anything other than zero.");
            timeStep = 0.02f; // Default to fixedupdate
        }

        Debug.Log($"Drawing orbit for {gameObject.name} from time {_networkedTime.ClientSimulationTime} to {stopTime} with time step {timeStep}");
        float t = _networkedTime.ClientSimulationTime;
        Vector3 p = transform.position;
        Vector3 oldP = p;
        Vector3 v = _velocity;
        GravitySource[] sources = FindObjectsByType<GravitySource>(FindObjectsSortMode.None);

        while (t < stopTime)
        {
            foreach (var source in sources)
            {
                Vector3 sourcePosition = source.transform.position;
                if (source.TryGetComponent<KeplerOrbit>(out var keplerOrbit))
                {
                    sourcePosition = keplerOrbit.CalculatePositionAtTime(t + keplerOrbit._timeOffset);
                    //Debug.Log($"KeplerOrbit object {keplerOrbit.name} position at time {t}: {sourcePosition}");
                }
                float m2 = source.mass;
                float r = Vector3.Distance(p, sourcePosition);

                v += (sourcePosition - p).normalized * (GravityConstant.G * (_baseMass * m2) / (r * r)) * timeStep * _networkedTime.TimeScale; // simplified gravitational equation
            }

            p += v * timeStep * _networkedTime.TimeScale;
            t += timeStep * _networkedTime.TimeScale;
            Debug.DrawLine(oldP, p, _orbitColor, 100f);
            //Debug.Log($"Drawing orbit line from {oldP} to {p} at time {t}");
            oldP = p;
        }
        _drawOrbit = false; 
    }
}
