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
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace test_websockets {
    public interface IWebSocketManager{
        ConcurrentDictionary<string, WebSocket> ConnectedSockets{get;set;}
        IObservable<MetricSnapshot> MetricsStream{get;}
        void OnMetricReceived(MetricSnapshot snapshot);
    }

  public class WebSocketManager : IWebSocketManager {
    private Subject<MetricSnapshot> _metricSubject;
    public WebSocketManager() {
        _metricSubject = new Subject<MetricSnapshot>();
        MetricsStream = _metricSubject;
        
    }
    public ConcurrentDictionary<string, WebSocket> ConnectedSockets {get;set;}

    public IObservable<MetricSnapshot> MetricsStream{get;}

    public void OnMetricReceived(MetricSnapshot snapshot){
        _metricSubject.OnNext(snapshot);
    }
  }

  public class Startup {
        private static ConcurrentDictionary<string, WebSocket> _websockets;
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
           services.AddMvc();
           services.AddSingleton(typeof(IWebSocketManager), new WebSocketManager());
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
            app.UseMvc();

           var wsMgr = app.ApplicationServices.GetService<IWebSocketManager>();

            _websockets = new ConcurrentDictionary<string, WebSocket>();

            wsMgr.ConnectedSockets = _websockets;


            app.Use( async (http, next) => {

                if (http.WebSockets.IsWebSocketRequest) {
                    var buffer = new ArraySegment<byte>(new byte[4096]);

                   var ws = await http.WebSockets.AcceptWebSocketAsync();
                   string id = Guid.NewGuid().ToString();

                   _websockets.TryAdd(id, ws);

                    while(ws.State == WebSocketState.Open) {
                        var result = await ws.ReceiveAsync(buffer, cancellationToken: CancellationToken.None);

                        if(result.MessageType == WebSocketMessageType.Close) {
                            WebSocket w;
                            _websockets.TryRemove(id, out w);
                            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by ws manager.", CancellationToken.None);
                            return;
                        }
                    }                        
                }
                else {
                    await next();
                }
            });

        }
    }

    public class MetricSnapshot {
        public string commandName { get; set; }
        public string noOfHosts{get;set;}
        public string commandTag{get;set;}
        public string errorPercentage{get;set;}
        public List<EventTypeAggregate> aggregates {get;set;}
        public long success{get;set;}
        public long failure {get;set;}
        public long timeout {get;set;}
        public long fallbackSuccess{get;set;}
        public long fallbackFailure{get;set;}
        public long shortCircuited{get;set;}
        public decimal latency_90th { get; set; }
        public decimal latency_99th { get; set; }
        public decimal latency_99_5th { get; set; }
        public decimal messagesPerSecond {get;set;}
        public string circuitBreakerStatus{get;set;}
        public string hostName{get;set;}
    }

    public class EventTypeAggregate {
        public string eventTypeName { get; set; }
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
