using UnityEngine;
using PurrNet;
using System;

public class KeplerOrbit : MonoBehaviour
{
    [SerializeField] private float _semiMajorAxis;
    [SerializeField] private float _eccentricity;
    [SerializeField] private float _inclination;
    [SerializeField] private GameObject _centralBody;
    [SerializeField] private float _ourMass;
    [SerializeField] private float _otherMass;
    public float _timeOffset;
    private NetworkedTime _networkedTime;

    private float _orbitalPeriod;
    private float _meanMotion;

    void Start()
    {
        _orbitalPeriod = 2f * Mathf.PI * Mathf.Sqrt(Mathf.Pow(_semiMajorAxis, 3f) / (GravityConstant.G * (_ourMass + _otherMass)));
        _meanMotion = 2f * Mathf.PI / _orbitalPeriod;

        _networkedTime = FindFirstObjectByType<NetworkedTime>();
    }

    void Update()
    {
        // Example usage: Calculate the position of the orbiting body at a specific time
        float timeSincePeriapsis = _networkedTime.ClientSimulationTime + _timeOffset; // Replace with your desired time

        Vector3 position = CalculatePositionAtTime(timeSincePeriapsis);
        transform.position = position;
    }

    public Vector3 CalculatePositionAtTime(float timeSincePeriapsis)
    {
        if (_semiMajorAxis <= 0f || _eccentricity < 0f || _eccentricity >= 1f)
        {
            return Vector3.zero;
        }

        float meanAnomaly = _meanMotion * timeSincePeriapsis;

        float eccentricAnomaly = SolveKeplerEquation(meanAnomaly, _eccentricity);
        float trueAnomaly = 2f * Mathf.Atan2(
            Mathf.Sqrt(1f + _eccentricity) * Mathf.Sin(eccentricAnomaly * 0.5f),
            Mathf.Sqrt(1f - _eccentricity) * Mathf.Cos(eccentricAnomaly * 0.5f));

        float radius = _semiMajorAxis * (1f - _eccentricity * Mathf.Cos(eccentricAnomaly));
        Vector3 localPosition = new Vector3(radius * Mathf.Cos(trueAnomaly), 0f, radius * Mathf.Sin(trueAnomaly));
        Vector3 inclinedPosition = Quaternion.Euler(0f, 0f, _inclination) * localPosition;

        return _centralBody != null
            ? _centralBody.transform.position + inclinedPosition
            : inclinedPosition;
    }

    private float SolveKeplerEquation(float meanAnomaly, float eccentricity)
    {
        float eccentricAnomaly = meanAnomaly;

        for (int i = 0; i < 10; i++)
        {
            float f = eccentricAnomaly - eccentricity * Mathf.Sin(eccentricAnomaly) - meanAnomaly;
            float fPrime = 1f - eccentricity * Mathf.Cos(eccentricAnomaly);
            float delta = f / fPrime;
            eccentricAnomaly -= delta;
        }

        return eccentricAnomaly;
    }
}
