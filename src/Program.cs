public interface INode
{
    public int[] Keys { get; set; }
    public int KeysCount { get; set; }
}

public class InternalNode : INode
{
    public int[] Keys { get; set; }
    public int KeysCount { get; set; }
    public INode[] Children { get; set; }

    public InternalNode(int order)
    {
        this.Keys = new int[order]; // should hold order - 1
        this.KeysCount = 0;
        this.Children = new INode[order + 1];
    }
}

public class LeafNode : INode
{
    public int[] Keys { get; set; }
    public int KeysCount { get; set; }
    public LeafNode? Prev { get; set; }
    public LeafNode? Next { get; set; }

    public LeafNode(int order, LeafNode? prev = null, LeafNode? next = null)
    {
        this.Keys = new int[order]; // should hold order - 1
        this.KeysCount = 0;
        this.Prev = prev;
        this.Next = next;
    }
}

public class SplitResult
{
    public int MedianKey { get; }
    public INode NewRightNode { get; }

    public SplitResult(int medianKey, INode newRightNode)
    {
        MedianKey = medianKey;
        NewRightNode = newRightNode;
    }
}

public class BPTree
{
    private int _order { get; set; }
    private INode? _root { get; set; }

    public BPTree(int order)
    {
        this._order = order;
        this._root = null;
    }

    public void Insert(int k)
    {
        if (_root == null) // on first insert
        {
            _root = new LeafNode(_order);
            _root.Keys[_root.KeysCount++] = k;
        }
        else
        {
            // Insert in it, then check if splits need to happen bottom up
            var splitResult = InsertAndSplit(_root, k);

            // If a split is needed at the top, the scenario changed, create new root
            if (splitResult != null)
            {
                InternalNode newRoot = new InternalNode(_order);
                newRoot.Keys[newRoot.KeysCount++] = splitResult.MedianKey;
                newRoot.Children[0] = _root;
                newRoot.Children[1] = splitResult.NewRightNode;

                _root = newRoot;
            }
        }
    }

    private SplitResult? InsertAndSplit(INode node, int k)
    {
        if (node is LeafNode lno)
        {
            InsertIntoLeaf(lno, k);

            // After insert, check if needs to split
            if (lno.KeysCount == _order)
            {
                return SplitLeaf(lno);
            }
        }
        else if (node is InternalNode ino)
        {
            // Find insert insert position
            int i = 0;
            while (i < ino.KeysCount && ino.Keys[i] < k)
            {
                i++;
            }

            // Recurse to child at that location to insert
            var splitResult = InsertAndSplit(ino.Children[i], k);

            // If you have to split this node, insert the right node and then split this one.
            if (splitResult != null)
            {
                InsertIntoInternal(ino, splitResult.MedianKey, splitResult.NewRightNode, i);

                if (ino.KeysCount == _order)
                {
                    return SplitInternal(ino);
                }
            }
        }
        else
        {
            throw new Exception("Unexpected node type");
        }

        return null;
    }

    private void InsertIntoLeaf(LeafNode node, int k)
    {
        // Find insertion and insert it, shifting the keys as needed
        int i = node.KeysCount - 1;

        while (i >= 0 && node.Keys[i] > k)
        {
            node.Keys[i + 1] = node.Keys[i];
            i--;
        }

        node.Keys[i + 1] = k;
        node.KeysCount++;
    }

    private void InsertIntoInternal(InternalNode node, int key, INode rightChild, int position)
    {
        // shift the keys and add the key to it
        for (int j = node.KeysCount; j > position; j--)
        {
            node.Keys[j] = node.Keys[j - 1];
            node.Children[j + 1] = node.Children[j];
        }

        node.Keys[position] = key;
        node.Children[position + 1] = rightChild;
        node.KeysCount++;
    }

    private SplitResult SplitLeaf(LeafNode node)
    {
        // Connect the links between leaf nodes
        LeafNode newRight = new LeafNode(_order, node, node.Next);
        if (node.Next != null) node.Next.Prev = newRight;
        node.Next = newRight;

        // Find middle index (median as it is sorted)
        int medianIndex = _order / 2;
        int medianKey = node.Keys[medianIndex];

        // Copy the right half of the left node to the right node
        for (int i = medianIndex; i < node.KeysCount; i++)
        {
            newRight.Keys[newRight.KeysCount++] = node.Keys[i];
            node.Keys[i] = 0; // Clear key data on left node
        }

        // Update left nodes key count to half
        node.KeysCount = medianIndex;

        return new SplitResult(medianKey, newRight);
    }

    private SplitResult SplitInternal(InternalNode node)
    {
        InternalNode newRight = new InternalNode(_order);

        // find middle (median as it is sorted)
        int medianIndex = _order / 2;
        int medianKey = node.Keys[medianIndex];

        // copy the left nodes right half into the right node
        for (int i = medianIndex + 1, j = 0; i < node.KeysCount; i++, j++)
        {
            newRight.Keys[j] = node.Keys[i];
            newRight.Children[j] = node.Children[i];
            newRight.KeysCount++;
        }

        // set the child from the left node to the right half after split
        newRight.Children[newRight.KeysCount] = node.Children[node.KeysCount];
        node.KeysCount = medianIndex; // reset the key count

        return new SplitResult(medianKey, newRight);
    }

    public void LevelOrderTraversal()
    {
        if (_root == null)
        {
            Console.WriteLine("Tree is empty.");
            return;
        }

        Queue<INode> queue = new Queue<INode>();
        queue.Enqueue(_root);

        while (queue.Count > 0)
        {
            int levelSize = queue.Count;

            for (int i = 0; i < levelSize; i++)
            {
                INode currentNode = queue.Dequeue();

                for (int j = 0; j < currentNode.KeysCount; j++)
                {
                    Console.Write(currentNode.Keys[j] + (currentNode.GetType() == typeof(InternalNode) ? "I" : "L") + " ");
                }
                Console.Write("| ");

                if (currentNode is InternalNode internalNode)
                {
                    for (int j = 0; j <= internalNode.KeysCount; j++)
                    {
                        if (internalNode.Children[j] != null)
                        {
                            queue.Enqueue(internalNode.Children[j]);
                        }
                    }
                }
            }

            Console.WriteLine();
        }
    }

    public void LeafListTraversal(int k, bool forward = true)
    {
        var node = FindNodeWithKey(_root, k);
        var tmp = node;

        Console.WriteLine($"Found node? : {tmp != null}");

        while (tmp != null)
        {
            for (int i = 0; i < tmp.KeysCount; i++)
            {
                Console.Write($"{tmp.Keys[i]}, ");
            }
            Console.Write(" -> ");

            if (forward)
            {
                tmp = tmp.Next;
            }
            else
            {
                tmp = tmp.Prev;
            }
        }
    }

    private LeafNode? FindNodeWithKey(INode? node, int key)
    {
        if (node == null)
        {
            return null;
        }

        int i = 0;

        while (i < node.KeysCount && key >= node.Keys[i])
        {
            i++;
        }

        if (node is LeafNode leaf)
        {
            for (int j = 0; j < leaf.KeysCount; j++)
            {
                Console.WriteLine("Searching " + leaf.Keys[j]);
                if (leaf.Keys[j] == key)
                {
                    return leaf;
                }
            }
            return null;
        }
        else if (node is InternalNode ino)
        {
            return FindNodeWithKey(ino.Children[i], key);
        }
        else
        {
            throw new Exception("Unexpected node type");
        }
    }
}

class Program
{
    public static void Main(string[] args)
    {
        BPTree tree = new BPTree(4);

        var arr = new int[50];
        for (int i = 1; i <= 50; i++)
        {
            arr[i - 1] = i;
        }

        Random random = new Random();

        for (int i = 0; i < arr.Length - 1; ++i)
        {
            int r = random.Next(i, arr.Length);
            (arr[r], arr[i]) = (arr[i], arr[r]);
        }

        foreach (var i in arr)
        {
            tree.Insert(i);
            Console.Write(i + " ");
        }
        Console.WriteLine();

        tree.LevelOrderTraversal();
        tree.LeafListTraversal(50, false);
    }
}