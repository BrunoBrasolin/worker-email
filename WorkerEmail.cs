using Gamidas.Utils.RabbitMQ.Receive;

namespace worker_email
{
    public class WorkerEmail : BackgroundService
    {
        private readonly IReceiveEvent _receiveEvent;

        public WorkerEmail(IReceiveEvent receiveEvent)
        {
            _receiveEvent = receiveEvent;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _receiveEvent.ReceiveEmail();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
