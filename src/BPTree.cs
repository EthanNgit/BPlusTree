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
        public void Insert(int key)
        {
            if (_root == null) // on first insert
            {
                _root = new LeafNode(_order);
                _root.Keys[_root.KeysCount++] = key;
            }
            else
            {
                // Insert in it, then check if splits need to happen bottom up
                var splitResult = InsertAndSplit(_root, key);

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

        private SplitResult? InsertAndSplit(INode node, int key)
        {
            if (node is LeafNode lNode)
            {
                InsertIntoLeaf(lNode, key);

                // After insert, check if needs to split
                if (lNode.KeysCount == _order)
                {
                    return SplitLeaf(lNode);
                }
            }
            else if (node is InternalNode iNode)
            {
                // Find insert insert position
                int i = 0;
                while (i < iNode.KeysCount && iNode.Keys[i] < key)
                {
                    i++;
                }

                // Recurse to child at that location to insert
                var splitResult = InsertAndSplit(iNode.Children[i], key);

                // If you have to split this node, insert the right node and then split this one.
                if (splitResult != null)
                {
                    InsertIntoInternal(iNode, splitResult.MedianKey, splitResult.NewRightNode, i);

                    if (iNode.KeysCount == _order)
                    {
                        return SplitInternal(iNode);
                    }
                }
            }
            else
            {
                throw new Exception("Error: Unexpected node type on InsertAndSplit");
            }

            return null;
        }

        private void InsertIntoLeaf(LeafNode node, int key)
        {
            // Find insertion and insert it, shifting the keys as needed
            int i = node.KeysCount - 1;

            while (i >= 0 && node.Keys[i] > key)
            {
                node.Keys[i + 1] = node.Keys[i];
                i--;
            }

            node.Keys[i + 1] = key;
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

            if (node is LeafNode lNode)
            {
                for (int j = 0; j < lNode.KeysCount; j++)
                {
                    if (lNode.Keys[j] == key)
                    {
                        return lNode;
                    }
                }
                return null;
            }
            else if (node is InternalNode iNode)
            {
                return FindNodeWithKey(iNode.Children[i], key);
            }
            else
            {
                throw new Exception("Error: Unexpected node type on FindNodeWithKey");
            }
        }

        public LeafNode? Search(int key)
        {
            return FindNodeWithKey(_root, key);
        }

        #endregion

        #region Delete
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

            if (node is LeafNode lNode)
            {
                // Find and delete the key
                int keyIndex = -1;
                for (int i = 0; i < lNode.KeysCount; i++)
                {
                    if (lNode.Keys[i] == key)
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
                for (int i = keyIndex; i < lNode.KeysCount - 1; i++)
                {
                    lNode.Keys[i] = lNode.Keys[i + 1];
                }
                lNode.KeysCount--;

                // Check if node needs rebalancing
                bool needsRebalancing = lNode.KeysCount < minKeys;
                return new DeleteResult(true, needsRebalancing);
            }
            else if (node is InternalNode iNode)
            {
                // Find the child node that should contain the key
                int childPos = 0;
                while (childPos < iNode.KeysCount && key >= iNode.Keys[childPos])
                {
                    childPos++;
                }

                var deleteResult = DeleteAndMerge(iNode.Children[childPos], key, iNode, childPos);

                // If key wasn't deleted, no further action needed
                if (!deleteResult.KeyDeleted)
                {
                    return deleteResult;
                }

                // Update parent key if it was changed due to borrowing
                if (deleteResult.BorrowedKey.HasValue && childPos > 0)
                {
                    iNode.Keys[childPos - 1] = deleteResult.BorrowedKey.Value;
                }

                // Handle rebalancing if needed
                if (deleteResult.NeedRebalancing)
                {
                    return RebalanceAfterDelete(iNode, childPos);
                }

                return new DeleteResult(true, false);
            }

            throw new Exception("Error: Unexpected node type on DeleteAndMerge");
        }

        private DeleteResult RebalanceAfterDelete(InternalNode parent, int childIndex)
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
            if (child is LeafNode lNode && leftSibling is LeafNode lLeftNode)
            {
                // Move the largest key from left sibling to child
                int borrowedKey = lLeftNode.Keys[lLeftNode.KeysCount - 1];
                InsertIntoLeaf(lNode, borrowedKey);
                lLeftNode.KeysCount--;

                // Update parent key
                parent.Keys[childIndex - 1] = lNode.Keys[0];
            }
            else if (child is InternalNode iNode && leftSibling is InternalNode iLeftNode)
            {
                // Move the parent key down
                ShiftRightAndInsertKey(iNode, parent.Keys[childIndex - 1], 0);

                // Move the last child pointer from left sibling
                ShiftRightAndInsertChild(iNode, iLeftNode.Children[iLeftNode.KeysCount], 0);

                // Move the largest key from left sibling to parent
                parent.Keys[childIndex - 1] = iLeftNode.Keys[iLeftNode.KeysCount - 1];

                iLeftNode.KeysCount--;
            }
        }

        private void BorrowFromRightSibling(InternalNode parent, int childIndex, INode child, INode rightSibling)
        {
            if (child is LeafNode lNode && rightSibling is LeafNode lRightNode)
            {
                // Move the smallest key from right sibling to child
                int borrowedKey = lRightNode.Keys[0];
                InsertIntoLeaf(lNode, borrowedKey);

                // Shift keys in right sibling
                for (int i = 0; i < lRightNode.KeysCount - 1; i++)
                {
                    lRightNode.Keys[i] = lRightNode.Keys[i + 1];
                }
                lRightNode.KeysCount--;

                // Update parent key
                parent.Keys[childIndex] = lRightNode.Keys[0];
            }
            else if (child is InternalNode iNode && rightSibling is InternalNode iRightNode)
            {
                // Move the parent key down
                iNode.Keys[iNode.KeysCount] = parent.Keys[childIndex];

                // Move the first child pointer from right sibling
                iNode.Children[iNode.KeysCount + 1] = iRightNode.Children[0];

                // Move the smallest key from right sibling to parent
                parent.Keys[childIndex] = iRightNode.Keys[0];

                // Shift keys and children in right sibling
                for (int i = 0; i < iRightNode.KeysCount - 1; i++)
                {
                    iRightNode.Keys[i] = iRightNode.Keys[i + 1];
                    iRightNode.Children[i] = iRightNode.Children[i + 1];
                }
                iRightNode.Children[iRightNode.KeysCount - 1] = iRightNode.Children[iRightNode.KeysCount];
                iRightNode.KeysCount--;

                iNode.KeysCount++;
            }
        }

        private void MergeWithLeftSibling(InternalNode parent, int childIndex)
        {
            INode leftSibling = parent.Children[childIndex - 1];
            INode child = parent.Children[childIndex];

            if (child is LeafNode lNode && leftSibling is LeafNode lLeftNode)
            {
                // Copy all keys from child to left sibling
                for (int i = 0; i < lNode.KeysCount; i++)
                {
                    lLeftNode.Keys[lLeftNode.KeysCount + i] = lNode.Keys[i];
                }
                lLeftNode.KeysCount += lNode.KeysCount;

                // Update leaf node links
                lLeftNode.Next = lNode.Next;
                if (lNode.Next != null)
                {
                    lNode.Next.Prev = lLeftNode;
                }
            }
            else if (child is InternalNode iNode && leftSibling is InternalNode iLeftNode)
            {
                // Move parent key to left sibling
                iLeftNode.Keys[iLeftNode.KeysCount] = parent.Keys[childIndex - 1];
                iLeftNode.KeysCount++;

                // Copy all keys and children from child to left sibling
                for (int i = 0; i < iNode.KeysCount; i++)
                {
                    iLeftNode.Keys[iLeftNode.KeysCount + i] = iNode.Keys[i];
                    iLeftNode.Children[iLeftNode.KeysCount + i] = iNode.Children[i];
                }
                iLeftNode.Children[iLeftNode.KeysCount + iNode.KeysCount] = iNode.Children[iNode.KeysCount];
                iLeftNode.KeysCount += iNode.KeysCount;
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

            if (child is LeafNode lNode && rightSibling is LeafNode lRightNode)
            {
                // Copy all keys from right sibling to child
                for (int i = 0; i < lRightNode.KeysCount; i++)
                {
                    lNode.Keys[lNode.KeysCount + i] = lRightNode.Keys[i];
                }
                lNode.KeysCount += lRightNode.KeysCount;

                // Update leaf node links
                lNode.Next = lRightNode.Next;
                if (lRightNode.Next != null)
                {
                    lRightNode.Next.Prev = lNode;
                }
            }
            else if (child is InternalNode iNode && rightSibling is InternalNode iRightNode)
            {
                // Move parent key to child
                iNode.Keys[iNode.KeysCount] = parent.Keys[childIndex];
                iNode.KeysCount++;

                // Copy all keys and children from right sibling to child
                for (int i = 0; i < iRightNode.KeysCount; i++)
                {
                    iNode.Keys[iNode.KeysCount + i] = iRightNode.Keys[i];
                    iNode.Children[iNode.KeysCount + i] = iRightNode.Children[i];
                }
                iNode.Children[iNode.KeysCount + iRightNode.KeysCount] = iRightNode.Children[iRightNode.KeysCount];
                iNode.KeysCount += iRightNode.KeysCount;
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
                    Console.Write(" | ");

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

            if (tmp == null)
            {
                Console.Write($"{k} not found.");
            }

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