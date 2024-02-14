namespace Poll.DAL.Services;

public class TransactionContext
{
    private List<Func<Task>> Callbacks = new List<Func<Task>>();

    public void RegisterAfterCommit(Func<Task> action)
    {
        Callbacks.Add(action);
    }

    public async Task OnAfterCommit(ILogger logger)
    {
        foreach (var callback in Callbacks)
        {
            try
            {
                await callback();
            }
            catch (Exception e)
            {
                logger.LogInformation(e, "Could not run commit");
            }
        }

        Callbacks.Clear();
    }
}