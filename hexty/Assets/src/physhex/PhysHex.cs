using System.Collections.Generic;
using UnityEngine;

namespace PhysHex
{

[System.Serializable]
public class AccruedVector3
{
    /// <summary>
    /// Total vector3 value from accrued vectors so far.
    /// </summary>
    public Vector3 Total { get { return m_Total; } }

    /// <summary>
    /// Scalar vector used to scale up every vector added to the total accrued value.
    /// </summary>
    public float Multiplier { get { return m_Multiplier; } set { m_Multiplier = value == 0 ? 1 : value; } }

    [SerializeField]
    private Vector3 m_Total = Vector3.zero;
    [SerializeField]
    private float m_Multiplier = 1;
    [SerializeField]
    private Dictionary<string, Vector3> m_Modifiers = new Dictionary<string, Vector3>();

    /// <summary>
    /// Default constructor that default initializes properties.
    /// </summary>
    public AccruedVector3() { }

    /// <summary>
    /// Constructor that sets the total accrued vector3 value to the supplied initial value.
    /// </summary>
    /// <param name="v">Initial accrued vector3 value.</param>
    public AccruedVector3(Vector3 v)
    {
        m_Total = v;
    }

    /// <summary>
    /// Accrue a vector3 value into the total.
    /// </summary>
    /// <param name="v">vector3 value to accrue to the total.</param>
    public void Accrue(Vector3 v)
    {
        m_Total += v * Multiplier;
    }

    /// <summary>
    /// Add a named vector3 accrued value to the total.
    /// The convenience of mapping a name to a vector3 accrued value
    /// allows callers to remove from the total any modifications
    /// done to this "named vector".
    /// If the name already exists, the supplied vector
    /// value is added to the base vector value and then that accrued into
    /// the total.
    /// If the name doesn't exist, the supplied vector value
    /// is accrued into the total.
    /// </summary>
    /// <param name="k">vector3 value name</param>
    /// <param name="v">vector3 value</param>
    public void Add(string k, Vector3 v)
    {
        if (m_Modifiers.ContainsKey(k))
        {
            m_Modifiers[k] += v;
        }
        else
        {
            m_Modifiers.Add(k, v);
        }

        Accrue(m_Modifiers[k]);
    }

    /// <summary>
    /// Removes all accrued value from the specified named value.
    /// </summary>
    /// <param name="k">vector3 value name</param>
    public void Remove(string k)
    {
        if (m_Modifiers.ContainsKey(k))
        {
            Accrue(-m_Modifiers[k]);
            m_Modifiers.Remove(k);
        }
    }

    /// <summary>
    /// Change the base vector3 value of a specific value name.
    /// </summary>
    /// <param name="k">vector3 value name.</param>
    /// <param name="v">vector3 value.</param>
    public void Set(string k, Vector3 v)
    {
        Remove(k);
        Add(k, v);
    }
}

/// <summary>
/// A particle is the simplest object that can be simulated in the physhex system.
/// </summary>
[System.Serializable]
public class Particle
{
    /// <summary>
    /// Holds the linear position of the particle in world space.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// Holds the linear velocity of the particle in world space.
    /// </summary>
    public Vector3 Velocity;

    /// <summary>
    /// Holds the acceleration of the particle in world space.
    /// </summary>
    public AccruedVector3 Acceleration;

    /// <summary>
    /// Holds the amount of damping applied to linear motion.
    /// Damping is required to remove energy added through numerical
    /// instability in the integrator.
    /// </summary>
    public float Damping;

    /// <summary>
    /// Holds the inverse of the mass of the particle.
    /// It is more useful to hold the inverse mass because
    /// integration is simpler and because in real-time simulation
    /// it is more useful to have objects with infinite mass (immovable)
    /// than zero mass (completely unstable in numerical simulation).
    /// </summary>
    public float InverseMass;

    /// <summary>
    /// Computes the mass of the particle based on the inverse mass.
    /// When setting via this property, InverseMass is the one set.
    /// </summary>
    public float Mass
    {
        get
        {
            return 1 / InverseMass;
        }

        set
        {
            InverseMass = 1 / value;
        }
    }

    /// <summary>
    /// Total force applied to this particle.
    /// Callers are expected to manage their own force manipulation.
    /// </summary>
    public AccruedVector3 Force;

    /// <summary>
    /// Default constructor of a particle at the origin without movement.
    /// </summary>
    public Particle()
    {
        Position = Vector3.zero;
        Velocity = Vector3.zero;
        Acceleration = new AccruedVector3();
        Damping = 0f;
        InverseMass = 0f;
        Force = new AccruedVector3();
    }

    /// <summary>
    /// Sets the InverseMass property based on the mass supplied in this function.
    /// </summary>
    public void SetMass(float mass)
    {
        InverseMass = 1 / mass;
    }

    /// <summary>
    /// Integrates the particle forward in time by the supplied duration.
    /// This function uses a Newton-Euler integration method, which is a linear approximation
    /// of the correct integral. For this reason it may be inaccurate in some cases.
    /// </summary>
    public void Integrate(float duration)
    {
        // Update linear position
        Position += Velocity * duration;

        // Work out the acceleration from the force
        var acceleration = Acceleration.Total + (Force.Total * InverseMass);

        // Update linear velocity from the acceleration
        Velocity += acceleration * duration;

        // Impose drag
        var d = Mathf.Pow(Damping, duration);
        Velocity *= d;
    }
}

}