using System.Threading.Tasks;

namespace NotificationMicroservice.Infrastructure.Services
{
    public interface IMessageBrokerService
    {
        Task StartAsync();
        Task StopAsync();
    }
} 