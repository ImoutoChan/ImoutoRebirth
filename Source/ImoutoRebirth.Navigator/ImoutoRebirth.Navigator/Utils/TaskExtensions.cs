using Serilog;

namespace ImoutoRebirth.Navigator.Utils;

public static class TaskExtensions
{
    extension(Task task)
    {
        public async void LogAndSuppressExceptions()
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

        public async void OnException(Action<Exception> onException)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                onException(e);
            }
        }
    }
}
