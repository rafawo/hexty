# PhysHex

**PhysHex** is a codename used to refer to a physics framework implementation based on Hexagons.

## Overview

**PhysHex** implements a physics system that utilizes an hexagonal grid as the basis for collision detection. Even though "physics" implementation is implied in the name, it doesn't really do over complicated real life physics simulation; in reality, it just provides a collision detection framework. This framework provides default collision resolution, but it's meant to be overriden for more flexible results.

In regular physics frameworks/engines, objects are described using some sort of polygon. Instead, all objects are described as a set of hexagons in an hexagonal grid, relative to the center of the object. The center is a 2D coordinate that is used to calculate its world position on an hexagonal grid and translate the relative hexagons into world hexagons.

Collision detection is thus resolved by calculating the intersection of all relevant objects' hexagon set. This framework supports multiple "layers" for a single object, where it can have a completely independent set of hexagons (still relative to the center). By default, collision detection is only done within the same layer, but the framework can be configured to enable collision between layers.

The result of a collision resolution is an offset to apply to the object's center. This result is aggregated through the collision detection and resolution of multiple collisions for all different layers in the objects.

By default, only hexagons using the same hex metrics can collide. The hex metrics is part of the object's configuration. Only one hex metric an be associated with an object. Support for collision between hexagons from different hex metrics can be enabled by specifying additional supported hex metrics on an object; these are not the main hex metric for the object, but are used to translate all "points" of an hexagon set into a new hexagon set with the new hex metric. ("Points" of an hexagon set are all those coordinates that construct the triangles that define each hexagon in the set).
