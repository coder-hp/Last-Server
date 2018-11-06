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
    class DoTask_Login
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            S2C_Login s2c = new S2C_Login();
            s2c.Tag = (int)CSParam.NetTag.Login;
            try
            {
                C2S_Login c2s = JsonConvert.DeserializeObject<C2S_Login>(data);

                if (!UserInfo.isExitsUser(c2s.DeviceId))
                {
                    Console.WriteLine("新增用户：" + c2s.DeviceId);
                    UserInfo.addUser(c2s.DeviceId);
                }

                s2c.Message = "登录成功";

                s2c.Id = c2s.DeviceId;
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
