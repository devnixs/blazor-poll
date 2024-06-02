using Microsoft.EntityFrameworkCore.Storage;

namespace Poll.DAL.Services;

public class PollExecutionStrategyFactory : RelationalExecutionStrategyFactory
{
    private readonly ExecutionStrategyDependencies _dependencies;

    public PollExecutionStrategyFactory(ExecutionStrategyDependencies dependencies)
        : base(dependencies)
    {
        _dependencies = dependencies;
    }

    protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyDependencies dependencies)
        => new PollExecutionStrategy(dependencies);

    public override IExecutionStrategy Create()
    {
        return new PollExecutionStrategy(_dependencies);
    }
}