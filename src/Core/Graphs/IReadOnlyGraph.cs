using System.Collections.Generic;

namespace BuildItEasy.Graphs
{
    public interface IReadOnlyGraph<TNode, TEdge>
        where TEdge : Edge<TNode>
    {
        IEnumerable<TNode> GetReachableNodes(TNode node);
        IEnumerable<TNode> GetReachingNodes(TNode node);
        IEnumerable<Path<TNode, TEdge>> GetPaths(TNode startNode, TNode endNode);
        IReadOnlyList<TNode> GetTopologicallySortedNodes();
    }
}
