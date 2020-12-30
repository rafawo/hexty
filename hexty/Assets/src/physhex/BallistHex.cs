using System;
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
[System.Serializable]
public class Projectile
{
    /// <summary>
    /// Physhex unit of simulation.
    /// </summary>
    public Particle Particle { get; private set;}

    /// <summary>
    /// Perishable traits of the projectile.
    /// Every time the projectile is set to a new particle,
    /// the Context property of the perishable member is
    /// set to the same particle object.
    /// </summary>
    public Perishable Perishable { get; private set; }

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
        Perishable = new Perishable {
            Epoch = 0,
            Expiry = expiry,
            Context = particle,
        };
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
        if (Perishable.Integrate(duration))
        {
            Particle.Integrate(duration);
        }
    }

    /// <summary>
    /// Represents a "null" projectile with an object that is still
    /// valid and can call Integrate to.
    /// However, the physhex simulation wouldn't do anything with
    /// this projectile.
    /// </summary>
    public static Projectile Nill = new Projectile(-1f, new Particle());
}

public static class ProjectileCommonTypeName
{
    public static string Pistol = "PISTOL";
    public static string Artillery = "ARTILLERY";
    public static string Fireball = "FIREBALL";
    public static string Laser = "LASER";
}

/// <summary>
/// Convenient repository of different projectiles
/// mapped by name that return a Projectile instance
/// correctly initialized to represent the physhex
/// necessary to simulate such ballistic object.
///
/// All instances of this class access the same underlying global
/// repository map. This means that it's a pseudo-singleton.
///
/// NOTE: The velocity and acceleration in the projectiles
/// is assuming a movement in the x-z plane, with no movement in the y axis.
/// Manual tinkering of the vectors is necessary to represent movement
/// on a different way.
/// </summary>
public class ProjectileRepository
{
    public Projectile this[string name]
    {
        get
        {
            lock (gm_Lock)
            {
                return gm_Projectiles.ContainsKey(name) ? gm_Projectiles[name] : Projectile.Nill;
            }
        }

        set
        {
            lock (gm_Lock)
            {
                if (gm_Projectiles.ContainsKey(name))
                {
                    gm_Projectiles[name] = value;
                }
                else
                {
                    gm_Projectiles.Add(name, value);
                }
            }
        }
    }

    private static object gm_Lock = new object();

    private static Dictionary<string, Projectile> gm_Projectiles = new Dictionary<string, Projectile>()
    {
        {
            ProjectileCommonTypeName.Pistol,
            new Projectile(
                float.MaxValue,
                new Particle {
                    Mass = 2f, // 2 kg
                    Velocity = new Vector3(0f, 0f, 35f), // 35 m/s
                    Acceleration = new AccruedVector3(new Vector3(0f, -1f, 0f)),
                    Damping = 0.99f,
                }
            )
        },
        {
            ProjectileCommonTypeName.Artillery,
            new Projectile(
                float.MaxValue,
                new Particle {
                    Mass = 200f, // 200 kg
                    Velocity = new Vector3(0f, 30f, 40f), // 50 m/s
                    Acceleration = new AccruedVector3(new Vector3(0f, -20f, 0f)),
                    Damping = 0.99f,
                }
            )
        },
        {
            ProjectileCommonTypeName.Fireball,
            new Projectile(
                float.MaxValue,
                new Particle {
                    Mass = 1f, // 1 kg - Mostly blast damage
                    Velocity = new Vector3(0f, 0f, 10f), // 5 m/s
                    Acceleration = new AccruedVector3(new Vector3(0f, 0.6f, 0f)),
                    Damping = 0.9f,
                }
            )
        },
        {
            ProjectileCommonTypeName.Laser,
            new Projectile(
                float.MaxValue,
                new Particle {
                    Mass = 0.1f, // 0.1 k - Almost no weight
                    Velocity = new Vector3(0f, 0f, 10f), // 100 m/s
                    Acceleration = new AccruedVector3(new Vector3(0f, 0f, 0f)), // No gravity
                    Damping = 0.9f,
                }
            )
        },
    };
}

}