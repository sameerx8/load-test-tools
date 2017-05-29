namespace test_websockets {
  using System.Net.WebSockets;
  using System.Collections.Generic;
  using System.Linq;
  using System.Collections.Concurrent;

  public class WebSocketManager{
    private readonly ConcurrentDictionary<string, WebSocket> _websockets = new ConcurrentDictionary<string, WebSocket>();

    public void Add(WebSocket socket){

    }


  }

  
}