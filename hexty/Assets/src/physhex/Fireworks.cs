// Copyright (c) 2020 Rafael Alcaraz Mercado. All rights reserved.
// Licensed under the MIT license <LICENSE-MIT or http://opensource.org/licenses/MIT>.
// All files in the project carrying such notice may not be copied, modified, or distributed
// except according to those terms.
// THE SOURCE CODE IS AVAILABLE UNDER THE ABOVE CHOSEN LICENSE "AS IS", WITH NO WARRANTIES.

using System.Collections.Generic;
using UnityEngine;

namespace PhysHex
{

[System.Serializable]
public class FireworkSpark
{
    public Projectile Projectile;
    public int CurrentPayload;
}

[System.Serializable]
public class FireworkPayload
{
    public float MinExpiry;
    public float MaxExpiry;
    public Vector3 MinVelocity;
    public Vector3 MaxVelocity;
    public float Damping;
    public int FuseCount;
    public bool AggregateParentVelocity;
    public bool UseParentDirection;

    public float RandomExpiry { get => Random.Range(MinExpiry, MaxExpiry); }
    public Vector3 RandomVelocity { get => new Vector3(Random.Range(MinVelocity.x, MaxVelocity.x), Random.Range(MinVelocity.y, MaxVelocity.y), Random.Range(MinVelocity.z, MaxVelocity.z)); }

    public List<FireworkSpark> Fuse(Particle parent, int payload)
    {
        var sparks = new List<FireworkSpark>();
        for (int i = 0; i < FuseCount; ++i)
        {
            var particle = parent.Clone();
            particle.Velocity = RandomVelocity;
            particle.Velocity += AggregateParentVelocity ? parent.Velocity : Vector3.zero;
            particle.Velocity = UseParentDirection ? parent.Velocity.normalized * particle.Velocity.magnitude : particle.Velocity;
            particle.Damping = Damping;
            var spark = new FireworkSpark {
                Projectile = new Projectile(RandomExpiry, particle.Velocity.normalized, particle),
                CurrentPayload = payload
            };
            spark.Projectile.Particle.Damping = Damping;
            sparks.Add(spark);
        }
        return sparks;
    }
}

[System.Serializable]
public class Firework
{
    public List<FireworkSpark> Sparks = new List<FireworkSpark>();
    public readonly List<FireworkPayload> Payloads;

    public Firework(List<FireworkPayload> payloads, Particle particle)
    {
        if (payloads.Count < 1)
        {
            throw new System.ArgumentException("Firework payload must have at least one entry");
        }

        Payloads = payloads;
        var proxy = new FireworkSpark { Projectile = new Projectile(0, Vector3.one, particle), CurrentPayload = 0 };
        Sparks = Payloads[0].Fuse(proxy.Projectile.Particle, 0);
    }

    private bool StepPayload(int index)
    {
        var spark = Sparks[index];
        if (index < Sparks.Count && spark.CurrentPayload < Payloads.Count)
        {
            Sparks.AddRange(Payloads[spark.CurrentPayload].Fuse(spark.Projectile.Particle, spark.CurrentPayload + 1));
            Sparks.RemoveAt(index);
            return true;
        }
        return false;
    }

    public bool Integrate(float duration)
    {
        if (Sparks == null) return false;
        var expiredIndexes = new List<int>();
        int index = -1;
        bool anyIntegrated = false;
        foreach (var s in Sparks)
        {
            ++index;
            if (!s.Projectile.Integrate(duration))
            {
                expiredIndexes.Add(index);
            }
            else
            {
                anyIntegrated = true;
            }
        }
        bool anyFused = false;
        foreach (var i in expiredIndexes)
        {
            anyFused |= StepPayload(i);
        }
        return anyIntegrated || anyFused;
    }
}

}