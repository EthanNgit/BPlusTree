using Norbula.BPTree;
using Norbula.BPTree.BPTreeTest;

class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("---------------------------------------------------------------");

        BPTree tree = new BPTree(4);
        var treeSize = 10;

        var arr = new int[treeSize];
        for (int i = 1; i <= treeSize; i++)
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

        foreach (var i in arr)
        {
            Console.WriteLine($"Deleting {i}\n");

            tree.Delete(i);

            tree.PrintLevelOrderTraversal();
            tree.PrintLeafListTraversal(1);

            Console.WriteLine("\n");
        }



        // TestBPTree.Run_All_Tests();
        Console.WriteLine("---------------------------------------------------------------");

    }
}