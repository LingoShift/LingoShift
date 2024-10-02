using Avalonia.Threading;
using LingoShift.Application.Interfaces;
using System.Threading.Tasks;
using System;

public class AvaloniaDispatcherService : IDispatcherService
{
    public async Task InvokeAsync(Action action)
    {
        await Dispatcher.UIThread.InvokeAsync(action);
    }

    public async Task<T> InvokeAsync<T>(Func<T> func)
    {
        return await Dispatcher.UIThread.InvokeAsync(func);
    }

    public Task<T> InvokeAsync<T>(Func<Task<T>> func)
    {
        return Dispatcher.UIThread.InvokeAsync(func, DispatcherPriority.Normal);
    }
}