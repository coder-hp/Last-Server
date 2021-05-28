using MyGameService.Game;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGameService.Net
{
    class DoTask_LeaveRoom
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            C2S_LeaveRoom c2s = JsonConvert.DeserializeObject<C2S_LeaveRoom>(data);

            RoomManager.deleteUser(clientInfo);
        }
    }
}
