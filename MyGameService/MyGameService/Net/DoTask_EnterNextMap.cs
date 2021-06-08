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
    class DoTask_EnterNextMap
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            C2S_EnterNextMap c2s = JsonConvert.DeserializeObject<C2S_EnterNextMap>(data);

            RoomManager.heroEnterNextMap(c2s.UserId);
        }
    }
}
