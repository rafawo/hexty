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

public class Firework
{
    public List<Projectile> Projectiles { get; private set; }
    public Queue<FireworkPayload> Payload { get; private set; }

    public Firework(Queue<FireworkPayload> payload, Vector3 direction, Particle particle)
    {
        Payload = payload ?? new Queue<FireworkPayload>();
        Projectiles = new List<Projectile>();
        Projectiles.Add(new Projectile(0, direction, particle));
        StepPayload();
    }

    private bool StepPayload()
    {
        var newList = new List<Projectile>();

        if (Payload.Count > 0)
        {
            var payload = Payload.Dequeue();

            foreach (var p in Projectiles)
            {
                for (var i = 0; i < payload.FuseCount; ++i)
                {
                    var particle = p.Particle.Clone();
                    particle.Velocity = new Vector3(
                        Random.Range(payload.MinVelocity.x, payload.MaxVelocity.x),
                        Random.Range(payload.MinVelocity.y, payload.MaxVelocity.y),
                        Random.Range(payload.MinVelocity.z, payload.MaxVelocity.z));
                    particle.Damping = payload.Damping;
                    newList.Add(new Projectile(Random.Range(payload.MinExpiry, payload.MaxExpiry), Vector3.one, particle));
                }
            }
        }

        Projectiles = newList;
        return Projectiles.Count > 0;
    }

    public bool Integrate(float duration)
    {
        bool anyIntegrated = false;

        foreach (var p in Projectiles)
        {
            if (p.Integrate(duration))
            {
                anyIntegrated = true;
            }
        }

        return !anyIntegrated && StepPayload() || anyIntegrated;
    }
}

}