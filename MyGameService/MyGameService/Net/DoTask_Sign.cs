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
    class DoTask_Sign
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            S2C_Sign s2c = new S2C_Sign();
            s2c.Tag = (int)CSParam.NetTag.Sign;
            try
            {
                C2S_Sign c2s = JsonConvert.DeserializeObject<C2S_Sign>(data);

                UserInfo.getUserInfoData(c2s.Id).Gold += 1000;
                s2c.Reward = "1:1000";

                Socket_S.getInstance().Send(clientInfo, s2c);
            }
            catch (Exception ex)
            {
                s2c.Message = "请求数据错误";
                Socket_S.getInstance().Send(clientInfo, s2c);
            }
        }
    }
}
