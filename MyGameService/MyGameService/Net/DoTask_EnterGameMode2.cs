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
    class DoTask_EnterGameMode2
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            S2C_EnterGameMode2 s2c = new S2C_EnterGameMode2();
            s2c.Tag = CSParam.NetTag.EnterGameMode2.ToString();
            s2c.Code = (int)CSParam.CodeType.Ok;
            Socket_S.getInstance().Send(clientInfo, s2c);

            C2S_EnterGameMode2 c2s = JsonConvert.DeserializeObject<C2S_EnterGameMode2>(data);
            MatchLogic.addUser(clientInfo, c2s);
        }
    }
}
