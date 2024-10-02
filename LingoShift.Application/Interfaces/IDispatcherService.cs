using System;
using System.Threading.Tasks;

namespace LingoShift.Application.Interfaces
{
    public interface IDispatcherService
    {
        Task InvokeAsync(Action action);
        Task<T> InvokeAsync<T>(Func<T> func);
        Task<T> InvokeAsync<T>(Func<Task<T>> func);
    }
}