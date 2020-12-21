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
    public Vector2 Velocity;

    /// <summary>
    /// Holds the acceleration of the particle in world space.
    /// </summary>
    public Vector2 Acceleration;
}

}