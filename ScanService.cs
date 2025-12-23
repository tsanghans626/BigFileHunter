namespace BigFileHunter;

public class ScanService
{
    public FolderNode? ScanDirectory(string? rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFound($"根目录不存在：{rootPath}");
        }

        // 1. 创建根节点
        FolderNode rootNode = new FolderNode(rootPath);
    
        // 2. 使用栈来辅助遍历
        Stack<FolderNode> stack = new();
        stack.Push(rootNode);

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

        return rootNode;
    }
}