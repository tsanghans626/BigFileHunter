namespace BigFileHunter;

public class ScanService
{
    private FolderNode? _rootNode;
    
    public void ScanDirectory(string? rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFound($"根目录不存在：{rootPath}");
        }

        // 1. 创建根节点
        _rootNode = new FolderNode(rootPath);
    
        // 2. 使用栈来辅助遍历
        Stack<FolderNode> stack = new();
        stack.Push(_rootNode);

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();

            try
            {
                // --- 处理文件：计算当前目录下的文件大小 ---
                DirectoryInfo di = new(currentNode.FullPath);
                foreach (var file in di.GetFiles())
                {
                    var fileSize = file.Length;
                
                    // 将文件大小累加给当前节点，以及它所有的祖先节点
                    var temp = currentNode;
                    while (temp != null)
                    {
                        temp.Size += fileSize;
                        temp = temp.Parent;
                    }
                }

                // --- 处理子目录：创建子节点并入栈 ---
                foreach (var dirPath in Directory.GetDirectories(currentNode.FullPath))
                {
                    FolderNode childNode = new(dirPath, currentNode);
                    currentNode.Children.Add(childNode);
                    stack.Push(childNode);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // 遇到系统保护文件夹，直接跳过
            }
        }
    }

    public void Print()
    {
        if (_rootNode == null)
        {
            Console.WriteLine("请先运行扫描！");
            return;
        }

        Console.WriteLine($"--- 磁盘扫描结果 ({_rootNode.FullPath}) ---");
        // 调用私有递归方法，从第 0 层开始
        PrintNode(_rootNode, 0);
    }

    public void PrintTopN(int n = 5)
    {
        if (_rootNode == null)
        {
            Console.WriteLine("请先运行扫描！");
            return;
        }

        var allNodes = new List<(FolderNode node, int depth)>();
        CollectNodes(_rootNode, 0, allNodes);

        var topNodes = allNodes
            .OrderByDescending(x => x.node.GetDirectSize())
            .Take(n)
            .ToList();

        Console.WriteLine($"--- Top {topNodes.Count} 文件夹（按直接文件大小） ---");

        int rank = 1;
        foreach (var (node, _) in topNodes)
        {
            Console.WriteLine($"{rank}. {node.FullPath} - 直接文件: {node.GetHDirectSize()}");
            rank++;
        }
    }

    private void PrintNode(FolderNode node, int indent)
    {
        // 1. 生成缩进字符串（每深一层多两个空格）
        string space = new string(' ', indent * 2);

        // 2. 打印当前节点信息
        // 这里使用了前面提到的 FormattedSize 的思路，简单起见先转成 MB
        Console.WriteLine($"{space} |- {node.Name} ({node.getHSize()}");

        // 3. 递归打印所有子节点
        // 进阶：可以先对 Children 按大小排序，这样打印出来就是"大头"在前
        foreach (var child in node.Children.OrderByDescending(c => c.Size))
        {
            PrintNode(child, indent + 1);
        }
    }

    private void CollectNodes(FolderNode node, int depth, List<(FolderNode node, int depth)> result)
    {
        result.Add((node, depth));

        foreach (var child in node.Children)
        {
            CollectNodes(child, depth + 1, result);
        }
    }
}