using Norbula.BPTree;

namespace Norbula.BPTree.BPTreeTest
{
    public class TestBPTree
    {
        private const int _testLoops = 10; // if random is used, this will be
        private const int _randomStartingSeed = 77;

        public static void Run_All_Tests()
        {
            Insert_Checks_Exists();
            Insert_100Elements_Checks_SortForward();
            Insert_100Elements_Checks_SortBackward();
            Insert_100Elements_Checks_CompleteDeletion();
        }

        public static void Insert_Checks_Exists()
        {
            string testName = "Insert_Checks_Exists";

            try
            {
                BPTree tree = new BPTree(4);
                tree.Insert(1);

                if (tree.Search(1) != null)
                {
                    Console.WriteLine($"PASS: {testName}");
                }
                else
                {
                    Console.WriteLine($"FAILED: {testName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: {testName} {ex}");
            }
        }

        public static void Insert_100Elements_Checks_SortForward()
        {
            string testName = "Insert_100Elements_Checks_SortForward";

            try
            {
                for (int b = 0; b < _testLoops; b++)
                {
                    // Arrange
                    BPTree tree = new BPTree(4);
                    var arr = new int[100];

                    for (int i = 1; i <= 100; i++) arr[i - 1] = i;

                    Random random = new Random(_randomStartingSeed + b);
                    for (int i = 0; i < arr.Length - 1; ++i)
                    {
                        int r = random.Next(i, arr.Length);
                        (arr[r], arr[i]) = (arr[i], arr[r]);
                    }

                    // Act
                    foreach (var i in arr) tree.Insert(i);
                    var nodeOne = tree.Search(1);

                    // Assert
                    var arrOne = new int[100];
                    var j = 0;
                    while (nodeOne != null)
                    {
                        var keys = nodeOne.Keys;

                        for (int k = 0; k < nodeOne.KeysCount; k++)
                        {
                            arrOne[j] = keys[k];

                            if (j > 0 && j < 100 && arrOne[j] < arrOne[j - 1])
                            {
                                Console.WriteLine($"FAILED: {testName} [seed: {_randomStartingSeed + b}]");
                                return;
                            }

                            j++;
                        }

                        nodeOne = nodeOne.Next;
                    }

                    continue; // passed this iteration
                }

                Console.WriteLine($"PASS: {testName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: {testName} {ex}");
            }
        }

        public static void Insert_100Elements_Checks_SortBackward()
        {
            string testName = "Insert_100Elements_Checks_SortBackward";

            try
            {
                // Arrange
                for (int b = 0; b < _testLoops; b++)
                {
                    BPTree tree = new BPTree(4);
                    var arr = new int[100];

                    for (int i = 1; i <= 100; i++) arr[i - 1] = i;

                    Random random = new Random(_randomStartingSeed + b);
                    for (int i = 0; i < arr.Length - 1; ++i)
                    {
                        int r = random.Next(i, arr.Length);
                        (arr[r], arr[i]) = (arr[i], arr[r]);
                    }

                    // Act
                    foreach (var i in arr) tree.Insert(i);
                    var nodeOne = tree.Search(100);

                    // Assert
                    var arrOne = new int[100];
                    var j = 0;
                    while (nodeOne != null)
                    {
                        var keys = nodeOne.Keys;

                        for (int k = nodeOne.KeysCount - 1; k >= 0; k--)
                        {
                            arrOne[j] = keys[k];
                            if (j > 0 && j < 100 && arrOne[j] > arrOne[j - 1])
                            {
                                Console.WriteLine($"FAILED: {testName} [seed: {_randomStartingSeed + b}]");
                                return;
                            }

                            j++;
                        }

                        nodeOne = nodeOne.Prev;
                    }

                    continue; // passed this iteration
                }

                Console.WriteLine($"PASS: {testName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: {testName} {ex}");
            }
        }

        public static void Insert_100Elements_Checks_CompleteDeletion()
        {
            string testName = "Insert_100Elements_Checks_CompleteDeletion";
            try
            {
                // Arrange
                for (int b = 0; b < _testLoops; b++)
                {
                    BPTree tree = new BPTree(4);
                    var arr = new int[100];
                    for (int i = 1; i <= 100; i++) arr[i - 1] = i;

                    Random random = new Random(_randomStartingSeed + b);
                    for (int i = 0; i < arr.Length - 1; ++i)
                    {
                        int r = random.Next(i, arr.Length);
                        (arr[r], arr[i]) = (arr[i], arr[r]);
                    }

                    // Act
                    foreach (var i in arr) tree.Insert(i);
                    var nodeOne = tree.Search(1);

                    // Assert
                    for (int i = 0; i < arr.Length - 1; ++i)
                    {
                        tree.Delete(i);

                        if (tree.Search(i) != null)
                        {
                            Console.WriteLine($"FAILED: {testName} [seed: {_randomStartingSeed + b}]");
                        }
                    }

                    continue; // passed this iteration
                }

                Console.WriteLine($"PASS: {testName}");
            }

            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: {testName} {ex}");
            }
        }
    }

}

