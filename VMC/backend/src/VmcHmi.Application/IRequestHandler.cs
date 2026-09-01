namespace VmcHmi.Application;

public interface IRequestHandler<TRequest, TResult>
{
    Task<TResult> HandleAsync(TRequest request, CancellationToken ct = default);
}

public interface IRequestHandler<TRequest>
{
    Task HandleAsync(TRequest request, CancellationToken ct = default);
}
