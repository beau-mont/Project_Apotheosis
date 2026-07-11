using System.Collections.Generic;
using System.Linq;
using PurrNet;
using UnityEngine;

public class GravityEntity : NetworkIdentity
{
    [SerializeField] private float m1;
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private float timeStep;

    void Start()
    {
        InitVelocity();
    }

    private void FixedUpdate()
    {
        GravitySource[] sources = FindObjectsByType<GravitySource>(FindObjectsSortMode.None);
        
        foreach (var source in sources)
        {
            float m2 = source.mass;
            float r = Vector3.Distance(transform.position, source.transform.position);

            _velocity += (source.transform.position - transform.position).normalized * (GravityConstant.G * (m1 * m2) / (r * r)) * Time.fixedDeltaTime * timeStep; // simplified gravitational equation
        }

        transform.position += _velocity * Time.fixedDeltaTime * timeStep;
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
}

