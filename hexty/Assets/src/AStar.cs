// Copyright (c) 2020 Rafael Alcaraz Mercado. All rights reserved.
// Licensed under the MIT license <LICENSE-MIT or http://opensource.org/licenses/MIT>.
// All files in the project carrying such notice may not be copied, modified, or distributed
// except according to those terms.
// THE SOURCE CODE IS AVAILABLE UNDER THE ABOVE CHOSEN LICENSE "AS IS", WITH NO WARRANTIES.

using System;
using System.Collections.Generic;

/// <summary>
/// Implementation of a generic priority queue.
/// </summary>
/// <typeparam name="T1"></typeparam>
public class PriorityQueue<T1>
{
    private List<Tuple<T1, double>> m_Elements = new List<Tuple<T1, double>>();

    /// <summary>
    /// Size of the queue.
    /// </summary>
    public int Count
    {
        get
        {
            return m_Elements.Count;
        }
    }

    /// <summary>
    /// Adds an element with its priority to the queue.
    /// </summary>
    /// <param name="Item">Item to enqueue.</param>
    /// <param name="Priority">Given priority.</param>
    public void Enqueue(T1 Item, double Priority)
    {
        m_Elements.Add(new Tuple<T1, double>(Item, Priority));
    }

    /// <summary>
    /// Returns the element with the highest priority.
    /// </summary>
    /// <returns>Element with the highest priority.</returns>
    public T1 Dequeue()
    {
        var bestIndex = GetHighestPriorityIndex();
        var bestItem = m_Elements[bestIndex].Item1;
        m_Elements.RemoveAt(bestIndex);
        return bestItem;
    }

    /// <summary>
    /// Shows the element with the highest priority.
    /// </summary>
    /// <returns>Reference to the elemet with the higuest priority.</returns>
    public T1 Peek()
    {
        return m_Elements[GetHighestPriorityIndex()].Item1;
    }

    /// <summary>
    /// Figures out which index corresponds to the item with the highest priority.
    /// </summary>
    /// <returns>Index to the higuest priority element.</returns>
    private int GetHighestPriorityIndex()
    {
        int bestIndex = 0;

        for (int i = 0; i < m_Elements.Count; i++)
        {
            if (m_Elements[i].Item2 < m_Elements[bestIndex].Item2)
            {
                bestIndex = i;
            }
        }

        return bestIndex;
    }
}

/// <summary>
/// Generic A* algorithm implementation that returns the shortest path between two nodes,
/// based on delegates instead of a tight dependency to a Graph type.
/// </summary>
/// <typeparam name="T">Data type of the nodes.</typeparam>
public static class AStar<T>
{
    /// <summary>
    /// Delegate used to retrieve all the neighbors of a given node.
    /// </summary>
    /// <param name="node">Node whose neighbors are returned.</param>
    /// <returns>Array of neighbors for a given node.</returns>
    public delegate T[] GetNeighbors(T node);

    /// <summary>
    /// Delegate used to retrieve the weight of a given node.
    /// </summary>
    /// <param name="node">Node whose weight is returned.</param>
    /// <returns>Weight of a given node.</returns>
    public delegate float GetWeight(T node);

    /// <summary>
    /// Delegate used to calculate the heuristic of the A* Algorithm.
    /// </summary>
    /// <param name="at">Node from where the heuristic starts.</param>
    /// <param name="to">Node where the heuristic is trying to get to.</param>
    /// <returns>Heuristic value for the supplied nodes.</returns>
    public delegate float Heuristic(T at, T to);

    /// <summary>
    /// Calculates the shortest path between two nodes.
    /// </summary>
    /// <param name="start">Starting node.</param>
    /// <param name="end">Ending node.</param>
    /// <param name="getNeighbors">Delegate that returns all neighbors of a given node.</param>
    /// <param name="getWeight">Delegate that returns the weight for a given node.</param>
    /// <param name="exists">Delegate that determines if a given node exists.</param>
    /// <param name="heuristic">Heuristic between two nodes.</param>
    /// <returns>Shortest path between two nodes. null if path doesn't exist.</returns>
    public static Queue<T> Search(
        T start,
        T end,
        GetNeighbors getNeighbors = null,
        GetWeight getWeight = null,
        Predicate<T> exists = null,
        Heuristic heuristic = null
        )
    {
        if (getNeighbors == null)
        {
            getNeighbors = x => new T[0];
        }

        if (getWeight == null)
        {
            getWeight = x => 1;
        }

        if (exists == null)
        {
            exists = x => true;
        }

        if (heuristic == null)
        {
            heuristic = (x, y) => 1;
        }

        var cameFrom = new Dictionary<T, T>();
        var costSoFar = new Dictionary<T, float>();
        var frontier = new PriorityQueue<T>();
        frontier.Enqueue(start, 0);

        cameFrom[start] = start;
        costSoFar[start] = 0;
        var current = default(T);

        while (frontier.Count > 0)
        {
            current = frontier.Dequeue();

            if (current.Equals(end))
            {
                break;
            }

            foreach (var next in getNeighbors(current))
            {
                if (!exists.Invoke(next))
                {
                    continue;
                }

                float newCost = costSoFar[current] + getWeight(next);
                if (!costSoFar.ContainsKey(next) || (newCost < costSoFar[next]))
                {
                    costSoFar[next] = newCost;
                    float priority = newCost + heuristic.Invoke(next, end);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(end))
        {
            return null;
        }

        Stack<T> pathList = new Stack<T>();
        current = end;
        pathList.Push(current);

        while (!current.Equals(start))
        {
            current = cameFrom[current];

            if (!current.Equals(start))
            {
                pathList.Push(current);
            }
        }

        Queue<T> path = new Queue<T>();
        while (pathList.Count > 0)
        {
            path.Enqueue(pathList.Pop());
        }

        return path;
    }
}
