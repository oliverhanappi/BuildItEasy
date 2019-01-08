namespace BuildItEasy.Utils.Graphs
{
    public class Edge<TNode>
    {
        public TNode StartNode { get; }
        public TNode EndNode { get; }

        public Edge(TNode startNode, TNode endNode)
        {
            StartNode = startNode;
            EndNode = endNode;
        }
    }
}
