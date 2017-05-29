using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebSockets.Server;
using System.Threading;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace test_websockets {
    public class Startup {
        private static ConcurrentDictionary<string, WebSocket> _websockets;
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseDefaultFiles();
            app.UseWebSockets();
            app.UseStaticFiles();

            app.Map("/metric", d => {
                app.Run(async context => {
                    await context.Response.WriteAsync("fk off");
                });
            });

            _websockets = new ConcurrentDictionary<string, WebSocket>();

            app.Use( async (http, next) => {
                var buffer = new ArraySegment<byte>(new byte[4096]);

                if(http.WebSockets.IsWebSocketRequest) {
                   var ws = await http.WebSockets.AcceptWebSocketAsync();
                   _websockets.TryAdd(Guid.NewGuid().ToString(), ws);
                   WebSocketReceiveResult result;

                   var snapshot = new MetricSnapshot {
                       commandName = Guid.NewGuid().ToString(),
                       aggregates = new List<EventTypeAggregate> {
                           new EventTypeAggregate { eventType = "Success", total = 100 }
                       },
                       latency_90th = 10,
                       latency_99_5th = 10,
                       latency_99th = 32,
                       messagesPerSecond = 32.3m
                   };

                   var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(snapshot));
                
                    await ws.SendAsync(new ArraySegment<byte>(data, 0, data.Length ),  WebSocketMessageType.Text,  true, CancellationToken.None);

                    while(ws.State== WebSocketState.Open){
                        do {
                            result = await ws.ReceiveAsync(buffer, CancellationToken.None);


                        } while(!result.EndOfMessage);
                    }
                   return;
                }
                
                await next();
            });

        }
    }

    public class MetricSnapshot {
        public string commandName { get; set; }
        public List<EventTypeAggregate> aggregates {get;set;}
        public decimal latency_90th { get; set; }
        public decimal latency_99th { get; set; }
        public decimal latency_99_5th { get; set; }
        public decimal messagesPerSecond {get;set;}
    }

    public class EventTypeAggregate {
        public string eventType { get; set; }
        public decimal total {get; set; }
    }

    public class Message {
        public string Hello{ get; set; }
    }

//     export class MetricSnapshotMessage{
//   commandName: string;
//   aggregates: EventTypeAggregate[];
//   latency_90th: number;
//   latency_99th: number;
//   latency_99_5th: number;
//   msgsPerSecond: number;
// }

// export class EventTypeAggregate{
//   eventType: string;
//   total: number;
// }
}
