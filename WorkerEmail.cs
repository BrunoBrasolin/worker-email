using Gamidas.Utils.RabbitMQ.Constants;
using Gamidas.Utils.RabbitMQ.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;

namespace worker_email
{
    public class WorkerEmail : BackgroundService
    {
        private readonly ILogger<WorkerEmail> _logger;
        private readonly IConfiguration _configuration;
        private readonly IModel _channel;

        public WorkerEmail(ILogger<WorkerEmail> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            ConnectionFactory factory = new()
            {
                HostName = _configuration["RabbitMQ:HostName"],
                Port = int.Parse(_configuration["RabbitMQ:Port"]),
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"],
            };

            IConnection _connection = factory.CreateConnection();
            _logger.LogInformation($"Created RabbitMQ Connection.");

             _channel = _connection.CreateModel();
            _logger.LogInformation($"Created RabbitMQ Model.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            EventingBasicConsumer consumer = new(_channel);

            consumer.Received += (model, ea) =>
            {
                byte[] body = ea.Body.ToArray();

                string json = Encoding.UTF8.GetString(body);

                EmailModel email = JsonConvert.DeserializeObject<EmailModel>(json);

                SmtpConfiguration smtpConfiguration = new(_configuration["SmtpConfiguration:Server"], Int32.Parse(_configuration["SmtpConfiguration:Port"]), _configuration["SmtpConfiguration:Username"], _configuration["SmtpConfiguration:AppPassword"]);

                SmtpClient smtpClient = new(smtpConfiguration.Server, smtpConfiguration.Port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpConfiguration.Username, smtpConfiguration.AppPassword)
                };

                MailMessage message = new(smtpConfiguration.Username, email.Recipient)
                {
                    Subject = email.Subject,
                    Body = email.Body
                };

                smtpClient.Send(message);
            };

            _channel.BasicConsume(autoAck: true, queue: QueueName.EmailQueue, consumer: consumer);
        }
    }

    class SmtpConfiguration
    {
        public string Server;
        public int Port;
        public string Username;
        public string AppPassword;

        public SmtpConfiguration(string server, int port, string username, string appPassword)
        {
            this.Server = server;
            this.Port = port;
            this.Username = username;
            this.AppPassword = appPassword;
        }
    }
}
