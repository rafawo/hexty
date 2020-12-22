using UnityEngine;

namespace PhysHex
{

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
    public Vector3 Acceleration;

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
    /// Total force applied to this particle.
    /// Callers are expected to manage their own force manipulation.
    /// </summary>
    public Vector3 Force;

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
        var acceleration = Acceleration + (Force * InverseMass);

        // Update linear velocity from the acceleration
        Velocity += acceleration * duration;

        // Impose drag
        var d = Mathf.Pow(Damping, duration);
        Velocity *= d;
    }

    /// <summary>
    /// Default constructor of a particle at the origin without movement.
    /// </summary>
    public Particle()
    {
        Position = Vector3.zero;
        Velocity = Vector3.zero;
        Acceleration = Vector3.zero;
        Damping = 0f;
        InverseMass = 0f;
    }
}

}