using Norbula.BPTree;

namespace Norbula.BPTree.BPTreeTest
{
    public class TestBPTree
    {
        public static void Run_All_Tests()
        {
            Insert_100Elements_ChecksSortForward();
            Insert_100Elements_ChecksSortBackward();
        }

        public static void Insert_100Elements_ChecksSortForward()
        {
            try
            {
                // Arrange
                BPTree tree = new BPTree(4);
                var arr = new int[100];

                for (int i = 1; i <= 100; i++) arr[i - 1] = i;

                Random random = new Random();
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
                            Console.WriteLine("FAILED: Insert_100Elements_ChecksSortForward");
                            return;
                        }

                        j++;
                    }

                    nodeOne = nodeOne.Next;
                }

                Console.WriteLine("PASS: Insert_100Elements_ChecksSortForward");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: Insert_100Elements_ChecksSortForward {ex}");
            }
        }

        public static void Insert_100Elements_ChecksSortBackward()
        {
            try
            {
                // Arrange
                BPTree tree = new BPTree(4);
                var arr = new int[100];

                for (int i = 1; i <= 100; i++) arr[i - 1] = i;

                Random random = new Random();
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
                            Console.WriteLine("FAILED: Insert_100Elements_ChecksSortBackward");
                            return;
                        }

                        j++;
                    }

                    nodeOne = nodeOne.Prev;
                }

                Console.WriteLine("PASS: Insert_100Elements_ChecksSortBackward");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: Insert_100Elements_ChecksSortBackward {ex}");
            }
        }


    }

}

