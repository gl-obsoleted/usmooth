
public class UsAppInfo
{
    static int _mainThreadId;

    static void Init() 
    {
        _mainThreadId = CurrentThreadID;
    }

    public static int CurrentThreadID
    {
        get { return System.Threading.Thread.CurrentThread.ManagedThreadId; }
    }

    public static bool IsMainThread
    {
        get { return CurrentThreadID == _mainThreadId; }
    }
}
