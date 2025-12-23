namespace BigFileHunter;

public interface IAppException
{
    public string ErrorCode { get; }
    public DateTime Timestamp { get; }
}


public class DirectoryNotFound(string message) : Exception, IAppException
{
    public string ErrorCode { get; } = "DirectoryNotFound";
    public DateTime Timestamp { get; } = DateTime.Now;
}