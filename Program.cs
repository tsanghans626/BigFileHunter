// See https://aka.ms/new-console-template for more information

using BigFileHunter;

Console.WriteLine("请输入文件夹路径：");
// /Users/tsanghans/Downloads
var folderPath = Console.ReadLine();

ScanService scanService = new();
scanService.ScanDirectory(folderPath);
scanService.PrintTopN();

