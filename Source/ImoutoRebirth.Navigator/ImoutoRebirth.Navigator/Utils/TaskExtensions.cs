using Serilog;

namespace ImoutoRebirth.Navigator.Utils;

public static class TaskExtensions
{
    public static async void LogAndSuppressExceptions(this Task task)
    {
        try
        {
            await task;
        }
        catch (Exception e)
        {
            Log.Error(e, "Exception in task");
        }
    }
}
