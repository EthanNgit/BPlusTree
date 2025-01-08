using Norbula.BPTree;
using Norbula.BPTree.BPTreeTest;

class Program
{
    public static void Main(string[] args)
    {
        // BPTree tree = new BPTree(4);

        // var arr = new int[50];
        // for (int i = 1; i <= 50; i++)
        // {
        //     arr[i - 1] = i;
        // }

        // Random random = new Random();

        // for (int i = 0; i < arr.Length - 1; ++i)
        // {
        //     int r = random.Next(i, arr.Length);
        //     (arr[r], arr[i]) = (arr[i], arr[r]);
        // }

        // foreach (var i in arr)
        // {
        //     tree.Insert(i);
        //     Console.Write(i + " ");
        // }
        // Console.WriteLine();

        // tree.LevelOrderTraversal();
        // tree.LeafListTraversal(50, false);

        TestBPTree.Run_All_Tests();
    }
}