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
    class DoTask_ExitMatchGameMode2
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            C2S_ExitMatchGameMode2 c2s = JsonConvert.DeserializeObject<C2S_ExitMatchGameMode2>(data);
            MatchLogic.removeUser(clientInfo);
            CommonUtil.Log("玩家退出匹配：" + c2s.UserId,true);
        }
    }
}
