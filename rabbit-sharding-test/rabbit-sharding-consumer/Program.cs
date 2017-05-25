using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace rabbit_sharding_consumer {
  class Program {
    static void Main(string[] args) {
      var factory = new ConnectionFactory
      {
        HostName = "sameersp3",
        UserName = "admin",
        Password = "admin",
        VirtualHost = "/"
      };

      var connection = factory.CreateConnection();

      var models = new List<IModel>();

      models.Add(CreateModel(connection));
      models.Add(CreateModel(connection));
      models.Add(CreateModel(connection));
      models.Add(CreateModel(connection));
      models.Add(CreateModel(connection));

      StartTiming().GetAwaiter().GetResult();

      Console.Read();

    }

    private static async Task StartTiming() {
      while (true) {
        Console.WriteLine($"Received {received}");
        await Task.Delay(1000);
      }
    }

    private static int received = 0;

    public static IModel CreateModel(IConnection connection) {
      var channel = connection.CreateModel();
      channel.BasicQos(0, 10, true);
      var consumer = new EventingBasicConsumer(channel);
      consumer.Received += (sender, args) => {
        Interlocked.Increment(ref received);
        channel.BasicAck(args.DeliveryTag, true);
      };
      channel.BasicConsume("sameer-test-1", false, consumer);
      return channel;
    }

  }
}
