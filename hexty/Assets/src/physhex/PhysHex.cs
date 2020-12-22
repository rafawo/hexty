using UnityEngine;

namespace PhysHex
{

/// <summary>
/// A particle is the simplest object that can be simulated in the physhex system.
/// </summary>
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
    /// Sets the InverseMass property based on the mass supplied in this function.
    /// </summary>
    public void SetMass(float mass)
    {
        InverseMass = 1 / mass;
    }

    /// <summary>
    /// Integrates the particle forward in time by the current time delta measured by Unity.
    /// This function uses a Newton-Euler integration method, which is a linear approximation
    /// of the correct integral. For this reason it may be inaccurate in some cases.
    /// </summary>
    public void Integrate(Vector3 force)
    {
        if (Time.deltaTime <= 0) return;

        // Update linear position
        Position += Velocity * Time.deltaTime;

        // Work out the acceleration from the force
        var acceleration = Acceleration + (force * Time.deltaTime);

        // Update linear velocity from the acceleration
        Velocity += acceleration * Time.deltaTime;

        // Impose drag
        Velocity *= Mathf.Pow(Damping, Time.deltaTime);
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
        SetMass(0f);
    }
}

}