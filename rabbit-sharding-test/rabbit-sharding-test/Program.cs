using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace rabbit_sharding_test {
  class Program {
    private static int sent;
    static void Main(string[] args) {
      var factory = new ConnectionFactory {
        HostName = "sameersp3",
        UserName = "admin",
        Password = "admin",
        VirtualHost = "/"
      };

      var connection = factory.CreateConnection();

      var models = new List<IModel>();
      
      models.Add(CreateModel(connection));

      var random = new Random();
      Task.Run(Timer);
      var messagesToSend = 10000;

      while (true) {
          foreach (var model in models) {
            for (int i = 0; i < messagesToSend; i++) {
              var props = model.CreateBasicProperties();
              props.Type = "deposit";
              props.MessageId = Guid.NewGuid().ToString();

              model.BasicPublish("sameer-test-1", $"sameer.{random.Next()}", true, props,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new {Id = Guid.NewGuid().ToString()})));
            }
            model.WaitForConfirmsOrDie();
            sent += messagesToSend;
          }
        Thread.Sleep(500);
      }
    }

    public static async Task Timer() {
      while (true) {
        Console.WriteLine($"Sent {sent}");
        await Task.Delay(1000);
      }
    }

    public static IModel CreateModel(IConnection connection) {
      var channel = connection.CreateModel();
      channel.ConfirmSelect();
      return channel;
    }

  }


}
