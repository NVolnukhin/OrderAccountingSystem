using System.Threading.Tasks;

namespace Shared.Contracts;

public interface IMessageHandler<T> where T : class
{
    Task HandleAsync(T message);
} 