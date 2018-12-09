using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildItEasy.Graphs
{
    public class Path<TNode, TEdge>
        where TEdge : Edge<TNode>
    {
        public TNode StartNode => Nodes.First();
        public TNode EndNode => Nodes.Last();
        
        public IReadOnlyList<TNode> Nodes { get; }
        public IReadOnlyList<TEdge> Edges { get; }

        public Path(IReadOnlyList<TEdge> edges)
        {
            if (edges == null)
                throw new ArgumentNullException(nameof(edges));

            if (edges.Count == 0)
                throw new ArgumentException("At least one edge is required.", nameof(edges));
            
            Edges = edges;

            var nodes = edges.Select(e => e.EndNode).ToList();
            nodes.Insert(0, edges[0].StartNode);

            Nodes = nodes;
        }

        public Path(TNode node)
        {
            Nodes = new[] {node};
            Edges = new TEdge[0];
        }

        public Path<TNode, TEdge> PrefixWith(TEdge edge)
        {
            return new Path<TNode, TEdge>(new[] {edge}.Concat(Edges).ToList());
        }
    }
}