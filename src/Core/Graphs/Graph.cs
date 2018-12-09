using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildItEasy.Graphs
{
    public class Graph<TNode, TEdge> : IReadOnlyGraph<TNode, TEdge>
        where TEdge : Edge<TNode>
    {
        private readonly IEqualityComparer<TNode> _nodeEqualityComparer;
        private readonly Dictionary<TNode, List<TEdge>> _outgoingEdgesByNode;
        private readonly Dictionary<TNode, List<TEdge>> _incomingEdgesByNode;

        public Graph(IEqualityComparer<TNode> nodeEqualityComparer = null)
        {
            _nodeEqualityComparer = nodeEqualityComparer ?? EqualityComparer<TNode>.Default;
            _outgoingEdgesByNode = new Dictionary<TNode, List<TEdge>>(_nodeEqualityComparer);
            _incomingEdgesByNode = new Dictionary<TNode, List<TEdge>>(_nodeEqualityComparer);
        }

        public void AddNode(TNode node)
        {
            if (_outgoingEdgesByNode.ContainsKey(node))
                throw new ArgumentException($"Node {node} has already been added.");

            _outgoingEdgesByNode.Add(node, new List<TEdge>());
            _incomingEdgesByNode.Add(node, new List<TEdge>());
        }

        public void AddEdge(TEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            AssertKnownNode(edge.StartNode);
            AssertKnownNode(edge.EndNode);

            if (_outgoingEdgesByNode[edge.StartNode].Any(e => _nodeEqualityComparer.Equals(e.EndNode, edge.EndNode)))
                throw new ArgumentException($"There is already an edge between {edge.StartNode} and {edge.EndNode}.");

            _outgoingEdgesByNode[edge.StartNode].Add(edge);
            _incomingEdgesByNode[edge.EndNode].Add(edge);
        }

        public IEnumerable<TNode> GetReachableNodes(TNode node)
        {
            AssertKnownNode(node);
            return GetReachableNodes(node, _outgoingEdgesByNode);
        }

        public IEnumerable<TNode> GetReachingNodes(TNode node)
        {
            AssertKnownNode(node);
            return GetReachableNodes(node, _incomingEdgesByNode);
        }

        private IEnumerable<TNode> GetReachableNodes(TNode node, IReadOnlyDictionary<TNode, List<TEdge>> edges)
        {
            var visitedEdges = new HashSet<TEdge>();
            var result = new HashSet<TNode>(_nodeEqualityComparer);

            Traverse(node);

            return result;

            void Traverse(TNode currentNode)
            {
                result.Add(currentNode);

                foreach (var edge in edges[currentNode])
                {
                    if (visitedEdges.Contains(edge))
                        continue;

                    visitedEdges.Add(edge);
                    Traverse(edge.EndNode);
                }
            }
        }

        public IEnumerable<Path<TNode, TEdge>> GetPaths(TNode startNode, TNode endNode)
        {
            AssertKnownNode(startNode);
            AssertKnownNode(endNode);

            var visitedEdges = new HashSet<TEdge>();

            return Traverse(startNode);

            IEnumerable<Path<TNode, TEdge>> Traverse(TNode currentNode)
            {
                if (_nodeEqualityComparer.Equals(currentNode, endNode))
                {
                    yield return new Path<TNode, TEdge>(currentNode);
                    yield break;
                }

                foreach (var edge in _outgoingEdgesByNode[currentNode])
                {
                    if (visitedEdges.Contains(edge))
                        continue;

                    visitedEdges.Add(edge);

                    foreach (var path in Traverse(edge.EndNode))
                    {
                        yield return path.PrefixWith(edge);
                    }
                }
            }
        }

        public IReadOnlyList<TNode> GetTopologicallySortedNodes()
        {
            var sorted = new List<TNode>();
            var visited = new Dictionary<TNode, bool>();

            foreach (var node in _outgoingEdgesByNode.Keys)
            {
                Visit(node);
            }

            return sorted;
            
            void Visit(TNode node)
            {
                var alreadyVisited = visited.TryGetValue(node, out var inProcess);
    
                if (alreadyVisited)
                {
                    if (inProcess)
                    {
                        throw new ArgumentException("Cyclic dependency found.");
                    }
                }
                else
                {
                    visited[node] = true;
    
                    var dependencies = _incomingEdgesByNode[node].Select(e => e.StartNode);
                    foreach (var dependency in dependencies)
                    {
                        Visit(dependency);
                    }
    
                    visited[node] = false;
                    sorted.Add(node);
                }
            }        
        }

        private void AssertKnownNode(TNode node)
        {
            if (!_outgoingEdgesByNode.ContainsKey(node))
                throw new ArgumentException($"Unknown node: {node}");
        }
    }
}
