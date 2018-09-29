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
    class DoTask_UserInfo
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            S2C_UserInfo s2c = new S2C_UserInfo();
            s2c.Tag = (int)Consts.NetTag.UserInfo;
            try
            {
                C2S_UserInfo c2s = JsonConvert.DeserializeObject<C2S_UserInfo>(data);

                s2c.UserInfoData = UserInfo.getUserInfoData(c2s.DeviceId);

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
