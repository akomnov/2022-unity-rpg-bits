using System.Collections.Generic;
using RPG.Core.Shared.Utils;

namespace RPG.Core.Shared.GraphSearch
{
    /// <summary>
    ///     Adapted from https://www.redblobgames.com/pathfinding/a-star/implementation.html
    ///     
    ///     TNode must implement GetHashCode and Equals correctly!
    /// </summary>
    public class AStar<TNode, TCost>
        where TCost : System.IComparable<TCost>
    {
        private readonly System.Func<TNode, IEnumerable<(TNode node, TCost cost)>> getNeighbors;
        private readonly System.Func<TNode, bool> goalTest;
        private readonly System.Func<TCost, TCost, TCost> addCost;
        private readonly System.Func<TNode, TCost> heuristicToGoal;
        private readonly PriorityQueue<(TNode node, TCost priority)> frontier;
        private readonly Dictionary<TNode, TNode> backLinks;
        private readonly Dictionary<TNode, TCost> frontierCosts;
        private TNode lastNode;
        public bool Successful { get; private set; }

        private class PriorityComparer : IComparer<(TNode, TCost priority)>
        {
            int IComparer<(TNode, TCost priority)>.Compare((TNode, TCost priority) a, (TNode, TCost priority) b)
            {
                return a.priority.CompareTo(b.priority);
            }
        }

        public AStar(
            TNode start,
            System.Func<TNode, IEnumerable<(TNode node, TCost cost)>> getNeighbors,
            System.Func<TNode, bool> goalTest,
            System.Func<TCost, TCost, TCost> addCost,
            System.Func<TNode, TCost> heuristicToGoal
        )
        {
            this.getNeighbors = getNeighbors;
            this.goalTest = goalTest;
            this.addCost = addCost;
            this.heuristicToGoal = heuristicToGoal;
            frontier = new PriorityQueue<(TNode, TCost)>(0, new PriorityComparer());
            frontier.Push((start, default(TCost)));
            frontierCosts = new Dictionary<TNode, TCost>{ [start] = default };
            backLinks = new Dictionary<TNode, TNode>();
        }

        /// <returns>True if should continue calling, false if found a path (Successful will be true) or never will.</returns>
        public bool RunIteration()
        {
            if (Successful || Exhausted()) return false;
            lastNode = frontier.Top.node;
            frontier.Pop();
            if (goalTest(lastNode))
            {
                Successful = true;
                return false;
            }
            foreach (var (_neighbor, _cost) in getNeighbors(lastNode))
            {
                TCost _newCost = addCost(frontierCosts[lastNode], _cost);
                if (!frontierCosts.ContainsKey(_neighbor) || _newCost.CompareTo(frontierCosts[_neighbor]) < 0) {
                    frontierCosts[_neighbor] = _newCost;
                    frontier.Push((_neighbor, addCost(_newCost, heuristicToGoal(_neighbor))));
                    backLinks[_neighbor] = lastNode;
                }
            }
            return true;
        }

        public bool Exhausted()
        {
            return frontier.Count == 0;
        }

        /// <returns>
        ///     [goal, start) list if the search was successful.
        ///     Empty list otherwise.
        /// </returns>
        public IList<TNode> Path()
        {
            if (!Successful) return new List<TNode>();
            TNode current = lastNode;
            var path = new List<TNode>{ current };
            while (backLinks.ContainsKey(current))
            {
                current = backLinks[current];
                path.Add(current);
            }
            return path;
        }
    }
}
