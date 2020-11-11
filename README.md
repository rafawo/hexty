# hexty
Hexagonal utilities tailored for Unity code

## Overview
This repository contains the implementation of random Hexagonal utilities meant to be used in Unity scripting. This means the code is C# and uses Unity's namespaces for a lot of things. This code has an end goal of morphing into a free Unity Asset that provides a rich Hexagonal utilities ecosystem that can be used for custom scenarios.

The two "byproducts" are:
1) **Hexagonal Math**: The utilities all provide the math behind Hexagonal grids and shapes, including geometry in terms of the hex grid. This opens up the storage design to the consumers of the utilities, and these are meant to generically solve the necessary calculations for Hexagonal usage.
2) **PhysHex**: A naive implementation of a physics framework where hexagons in an hexagonal grid represent rigid bodies.
