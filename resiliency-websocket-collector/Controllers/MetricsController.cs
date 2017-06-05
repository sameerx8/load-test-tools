using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace test_websockets {
  public class MetricsController : Controller {
    private IWebSocketManager _websocketManager;
    public MetricsController(IWebSocketManager websocketManager) {
        _websocketManager = websocketManager;
    }
    
    [Route("api/metrics")]
    public async Task<IActionResult> PostMetrics([FromBody]MetricSnapshot snapshot) {

        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(snapshot));

        foreach( var socket in _websocketManager.ConnectedSockets){
          try{
            await socket.Value.SendAsync(new ArraySegment<byte>(data, 0, data.Length ),  WebSocketMessageType.Text,  true, CancellationToken.None);
          }
          catch(Exception ex){
            _websocketManager.ConnectedSockets.TryRemove(socket.Key, out var sock);
          }
        }
        
        return Ok();
    }
  }
}