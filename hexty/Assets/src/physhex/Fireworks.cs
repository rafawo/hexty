// Copyright (c) 2020 Rafael Alcaraz Mercado. All rights reserved.
// Licensed under the MIT license <LICENSE-MIT or http://opensource.org/licenses/MIT>.
// All files in the project carrying such notice may not be copied, modified, or distributed
// except according to those terms.
// THE SOURCE CODE IS AVAILABLE UNDER THE ABOVE CHOSEN LICENSE "AS IS", WITH NO WARRANTIES.

using System.Collections.Generic;
using UnityEngine;

namespace PhysHex
{

public class FireworkPayload
{
    public float MinExpiry { get; private set; }
    public float MaxExpiry { get; private set; }
    public Vector3 MinVelocity { get; private set; }
    public Vector3 MaxVelocity { get; private set; }
    public float Damping { get; private set; }
    public int FuseCount { get; private set; }
}

public class FireworkSpark
{
    public Projectile Projectile;
    public int CurrentPayload;
}

public class Firework
{
    public List<FireworkSpark> Sparks { get; private set; }
    public List<FireworkPayload> Payload { get; private set; }

    public Firework(List<FireworkPayload> payload, Vector3 direction, Particle particle)
    {
        Payload = payload ?? new List<FireworkPayload>();
        Sparks = new List<FireworkSpark>();
        Sparks.Add(new FireworkSpark {
            Projectile = new Projectile(0, direction, particle),
            CurrentPayload = 0
        });
        StepPayload(0);
    }

    private bool StepPayload(int index)
    {
        if (index < Sparks.Count && Sparks[index].CurrentPayload < Payload.Count)
        {
            var payload = Payload[Sparks[index].CurrentPayload];
            for (var i = 0; i < payload.FuseCount; ++i)
            {
                var particle = Sparks[index].Projectile.Particle.Clone();
                particle.Velocity = new Vector3(
                    Random.Range(payload.MinVelocity.x, payload.MaxVelocity.x),
                    Random.Range(payload.MinVelocity.y, payload.MaxVelocity.y),
                    Random.Range(payload.MinVelocity.z, payload.MaxVelocity.z));
                particle.Damping = payload.Damping;
                Sparks.Add(new FireworkSpark{
                    Projectile = new Projectile(Random.Range(payload.MinExpiry, payload.MaxExpiry), Vector3.one, particle),
                    CurrentPayload = Sparks[index].CurrentPayload + 1,
                });
            }
            Sparks.RemoveAt(index);
        }
        return Sparks.Count > 0;
    }

    public bool Integrate(float duration)
    {
        var expiredIndexes = new List<int>();
        int index = -1;
        foreach (var s in Sparks)
        {
            ++index;
            if (!s.Projectile.Particle.Integrate(duration))
            {
                expiredIndexes.Add(index);
            }
        }
        foreach (var i in expiredIndexes)
        {
            StepPayload(i);
        }
        return Sparks.Count > 0;
    }
}

}