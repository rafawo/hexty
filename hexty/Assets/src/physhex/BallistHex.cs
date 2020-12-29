using System.Collections.Generic;
using UnityEngine;

namespace PhysHex
{

/// <summary>
/// Class that represents a "projectile" ballistic object.
/// The functionality of this class could be generically be used
/// for other things that are temporary and have an initial particle
/// configuration when created.
/// </summary>
public class Projectile
{
    /// <summary>
    /// Physhex unit of simulation.
    /// </summary>
    public Particle Particle;

    /// <summary>
    /// Accumulated duration since the projectile started
    /// (either construction or reset).
    /// </summary>
    public float Epoch { get { return m_Epoch; } }

    /// <summary>
    /// Expiry threshold for the epoch to stop calling
    /// into the particle's integrate method.
    /// The particle can still be manually integrated
    /// via the public Particle property.
    /// </summary>
    public float Expiry { get { return m_Expiry; } }

    private float m_Epoch;
    private float m_Expiry;

    /// <summary>
    /// Constructor that configures a projectile with an expiry threshold
    /// and a particle object describing the unit of physhex simulation
    /// the projectile starts with (mass, velocity, acceleration, damping).
    /// </summary>
    /// <param name="expiry">Expiration threshold value.</param>
    /// <param name="particle">Physhex unit of simulation starting point.</param>
    public Projectile(float expiry, Particle particle)
    {
        Reset(expiry, particle);
    }

    /// <summary>
    /// Reconfigures the project with a new expiry threshold
    /// and a new particle object describing the unit of physhex simulation
    /// the projectile starts with (mass, velocity, acceleration, damping).
    /// </summary>
    /// <param name="expiry">Expiration threshold value.</param>
    /// <param name="particle">Physhex unit of simulation starting point.</param>
    public void Reset(float expiry, Particle particle)
    {
        m_Epoch = 0;
        m_Expiry = expiry;
        Particle = particle;
    }

    /// <summary>
    /// Integrate the projectile by integrating the underlying
    /// particle used to simulate position movement.
    /// If the epoch has gone over the threshold then
    /// this doesn't integrate the particle.
    /// </summary>
    /// <param name="duration">Supplies the duration of time units that have occured since the last time.</param>
    public void Integrate(float duration)
    {
        m_Epoch += duration;
        if (m_Epoch <= m_Expiry)
        {
            Particle.Integrate(duration);
        }
    }
}

}