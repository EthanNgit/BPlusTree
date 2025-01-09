namespace Norbula.BPTree
{
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

    public class DeleteResult
    {
        public bool KeyDeleted { get; set; }
        public bool NeedRebalancing { get; set; }
        public int? BorrowedKey { get; set; }

        public DeleteResult(bool keyDeleted, bool needRebalancing, int? borrowedKey = null)
        {
            KeyDeleted = keyDeleted;
            NeedRebalancing = needRebalancing;
            BorrowedKey = borrowedKey;
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

        #region Insert
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

        #endregion

        #region Search
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

        public LeafNode? Search(int key)
        {
            return FindNodeWithKey(_root, key);
        }

        #endregion

        #region  Delete
        public void Delete(int key)
        {
            if (_root == null) return;

            var deleteResult = DeleteAndMerge(_root, key, null, -1);

            // If root becomes empty after deletion and it's an internal node
            if (_root is InternalNode && _root.KeysCount == 0)
            {
                _root = ((InternalNode)_root).Children[0];
            }
            // If root is a leaf and becomes empty, set root to null
            else if (_root is LeafNode && _root.KeysCount == 0)
            {
                _root = null;
            }
        }

        private DeleteResult DeleteAndMerge(INode node, int key, InternalNode? parent, int childIndex)
        {
            int minKeys = (_order - 1) / 2;

            if (node is LeafNode leaf)
            {
                // Find and delete the key
                int keyIndex = -1;
                for (int i = 0; i < leaf.KeysCount; i++)
                {
                    if (leaf.Keys[i] == key)
                    {
                        keyIndex = i;
                        break;
                    }
                }

                // If key not found, return
                if (keyIndex == -1)
                {
                    return new DeleteResult(false, false);
                }

                // Remove the key by shifting remaining keys
                for (int i = keyIndex; i < leaf.KeysCount - 1; i++)
                {
                    leaf.Keys[i] = leaf.Keys[i + 1];
                }
                leaf.KeysCount--;

                // Check if node needs rebalancing
                bool needsRebalancing = leaf.KeysCount < minKeys;
                return new DeleteResult(true, needsRebalancing);
            }
            else if (node is InternalNode internal_node)
            {
                // Find the child node that should contain the key
                int childPos = 0;
                while (childPos < internal_node.KeysCount && key >= internal_node.Keys[childPos])
                {
                    childPos++;
                }

                var deleteResult = DeleteAndMerge(internal_node.Children[childPos], key, internal_node, childPos);

                // If key wasn't deleted, no further action needed
                if (!deleteResult.KeyDeleted)
                {
                    return deleteResult;
                }

                // Update parent key if it was changed due to borrowing
                if (deleteResult.BorrowedKey.HasValue && childPos > 0)
                {
                    internal_node.Keys[childPos - 1] = deleteResult.BorrowedKey.Value;
                }

                // Handle rebalancing if needed
                if (deleteResult.NeedRebalancing)
                {
                    return RebalanceAfterDelete(internal_node, childPos, parent, childIndex);
                }

                return new DeleteResult(true, false);
            }

            throw new Exception("Unexpected node type");
        }

        private DeleteResult RebalanceAfterDelete(InternalNode parent, int childIndex, InternalNode? grandParent, int parentIndex)
        {
            int minKeys = (_order - 1) / 2;
            INode child = parent.Children[childIndex];

            // Try borrowing from left sibling
            if (childIndex > 0)
            {
                INode leftSibling = parent.Children[childIndex - 1];
                if (leftSibling.KeysCount > minKeys)
                {
                    BorrowFromLeftSibling(parent, childIndex, child, leftSibling);
                    return new DeleteResult(true, false);
                }
            }

            // Try borrowing from right sibling
            if (childIndex < parent.KeysCount)
            {
                INode rightSibling = parent.Children[childIndex + 1];
                if (rightSibling.KeysCount > minKeys)
                {
                    BorrowFromRightSibling(parent, childIndex, child, rightSibling);
                    return new DeleteResult(true, false);
                }
            }

            // If borrowing not possible, merge with a sibling
            if (childIndex > 0)
            {
                MergeWithLeftSibling(parent, childIndex);
            }
            else
            {
                MergeWithRightSibling(parent, childIndex);
            }

            // Check if parent needs rebalancing
            bool parentNeedsRebalancing = parent.KeysCount < minKeys;
            return new DeleteResult(true, parentNeedsRebalancing);
        }

        private void BorrowFromLeftSibling(InternalNode parent, int childIndex, INode child, INode leftSibling)
        {
            if (child is LeafNode leafNode && leftSibling is LeafNode leftLeaf)
            {
                // Move the largest key from left sibling to child
                int borrowedKey = leftLeaf.Keys[leftLeaf.KeysCount - 1];
                InsertIntoLeaf(leafNode, borrowedKey);
                leftLeaf.KeysCount--;

                // Update parent key
                parent.Keys[childIndex - 1] = leafNode.Keys[0];
            }
            else if (child is InternalNode internalNode && leftSibling is InternalNode leftInternal)
            {
                // Move the parent key down
                ShiftRightAndInsertKey(internalNode, parent.Keys[childIndex - 1], 0);

                // Move the last child pointer from left sibling
                ShiftRightAndInsertChild(internalNode, leftInternal.Children[leftInternal.KeysCount], 0);

                // Move the largest key from left sibling to parent
                parent.Keys[childIndex - 1] = leftInternal.Keys[leftInternal.KeysCount - 1];

                leftInternal.KeysCount--;
            }
        }

        private void BorrowFromRightSibling(InternalNode parent, int childIndex, INode child, INode rightSibling)
        {
            if (child is LeafNode leafNode && rightSibling is LeafNode rightLeaf)
            {
                // Move the smallest key from right sibling to child
                int borrowedKey = rightLeaf.Keys[0];
                InsertIntoLeaf(leafNode, borrowedKey);

                // Shift keys in right sibling
                for (int i = 0; i < rightLeaf.KeysCount - 1; i++)
                {
                    rightLeaf.Keys[i] = rightLeaf.Keys[i + 1];
                }
                rightLeaf.KeysCount--;

                // Update parent key
                parent.Keys[childIndex] = rightLeaf.Keys[0];
            }
            else if (child is InternalNode internalNode && rightSibling is InternalNode rightInternal)
            {
                // Move the parent key down
                internalNode.Keys[internalNode.KeysCount] = parent.Keys[childIndex];

                // Move the first child pointer from right sibling
                internalNode.Children[internalNode.KeysCount + 1] = rightInternal.Children[0];

                // Move the smallest key from right sibling to parent
                parent.Keys[childIndex] = rightInternal.Keys[0];

                // Shift keys and children in right sibling
                for (int i = 0; i < rightInternal.KeysCount - 1; i++)
                {
                    rightInternal.Keys[i] = rightInternal.Keys[i + 1];
                    rightInternal.Children[i] = rightInternal.Children[i + 1];
                }
                rightInternal.Children[rightInternal.KeysCount - 1] = rightInternal.Children[rightInternal.KeysCount];
                rightInternal.KeysCount--;

                internalNode.KeysCount++;
            }
        }

        private void MergeWithLeftSibling(InternalNode parent, int childIndex)
        {
            INode leftSibling = parent.Children[childIndex - 1];
            INode child = parent.Children[childIndex];

            if (child is LeafNode leafNode && leftSibling is LeafNode leftLeaf)
            {
                // Copy all keys from child to left sibling
                for (int i = 0; i < leafNode.KeysCount; i++)
                {
                    leftLeaf.Keys[leftLeaf.KeysCount + i] = leafNode.Keys[i];
                }
                leftLeaf.KeysCount += leafNode.KeysCount;

                // Update leaf node links
                leftLeaf.Next = leafNode.Next;
                if (leafNode.Next != null)
                {
                    leafNode.Next.Prev = leftLeaf;
                }
            }
            else if (child is InternalNode internalNode && leftSibling is InternalNode leftInternal)
            {
                // Move parent key to left sibling
                leftInternal.Keys[leftInternal.KeysCount] = parent.Keys[childIndex - 1];
                leftInternal.KeysCount++;

                // Copy all keys and children from child to left sibling
                for (int i = 0; i < internalNode.KeysCount; i++)
                {
                    leftInternal.Keys[leftInternal.KeysCount + i] = internalNode.Keys[i];
                    leftInternal.Children[leftInternal.KeysCount + i] = internalNode.Children[i];
                }
                leftInternal.Children[leftInternal.KeysCount + internalNode.KeysCount] = internalNode.Children[internalNode.KeysCount];
                leftInternal.KeysCount += internalNode.KeysCount;
            }

            // Remove parent key and child pointer
            for (int i = childIndex - 1; i < parent.KeysCount - 1; i++)
            {
                parent.Keys[i] = parent.Keys[i + 1];
                parent.Children[i + 1] = parent.Children[i + 2];
            }
            parent.KeysCount--;
        }

        private void MergeWithRightSibling(InternalNode parent, int childIndex)
        {
            INode child = parent.Children[childIndex];
            INode rightSibling = parent.Children[childIndex + 1];

            if (child is LeafNode leafNode && rightSibling is LeafNode rightLeaf)
            {
                // Copy all keys from right sibling to child
                for (int i = 0; i < rightLeaf.KeysCount; i++)
                {
                    leafNode.Keys[leafNode.KeysCount + i] = rightLeaf.Keys[i];
                }
                leafNode.KeysCount += rightLeaf.KeysCount;

                // Update leaf node links
                leafNode.Next = rightLeaf.Next;
                if (rightLeaf.Next != null)
                {
                    rightLeaf.Next.Prev = leafNode;
                }
            }
            else if (child is InternalNode internalNode && rightSibling is InternalNode rightInternal)
            {
                // Move parent key to child
                internalNode.Keys[internalNode.KeysCount] = parent.Keys[childIndex];
                internalNode.KeysCount++;

                // Copy all keys and children from right sibling to child
                for (int i = 0; i < rightInternal.KeysCount; i++)
                {
                    internalNode.Keys[internalNode.KeysCount + i] = rightInternal.Keys[i];
                    internalNode.Children[internalNode.KeysCount + i] = rightInternal.Children[i];
                }
                internalNode.Children[internalNode.KeysCount + rightInternal.KeysCount] = rightInternal.Children[rightInternal.KeysCount];
                internalNode.KeysCount += rightInternal.KeysCount;
            }

            // Remove parent key and child pointer
            for (int i = childIndex; i < parent.KeysCount - 1; i++)
            {
                parent.Keys[i] = parent.Keys[i + 1];
                parent.Children[i + 1] = parent.Children[i + 2];
            }
            parent.KeysCount--;
        }

        private void ShiftRightAndInsertKey(InternalNode node, int key, int position)
        {
            for (int i = node.KeysCount; i > position; i--)
            {
                node.Keys[i] = node.Keys[i - 1];
            }
            node.Keys[position] = key;
            node.KeysCount++;
        }

        private void ShiftRightAndInsertChild(InternalNode node, INode child, int position)
        {
            for (int i = node.KeysCount + 1; i > position; i--)
            {
                node.Children[i] = node.Children[i - 1];
            }
            node.Children[position] = child;
        }

        #endregion

        #region Print
        public void PrintLevelOrderTraversal()
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

        public void PrintLeafListTraversal(int k, bool forward = true)
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
        #endregion
    }
}