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
    class DoTask_SubmitState
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            C2S_SubmitState c2s = JsonConvert.DeserializeObject<C2S_SubmitState>(data);

            RoomLogic roomLogic = RoomManager.getRoomByUserId(c2s.UserId);
            if(roomLogic != null)
            {
                roomLogic.broadcastState(c2s);
            }
        }
    }
}
