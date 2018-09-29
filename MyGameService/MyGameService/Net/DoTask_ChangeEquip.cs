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
    class DoTask_ChangeEquip
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            S2C_ChangeEquip s2c = new S2C_ChangeEquip();
            s2c.Tag = (int)Consts.NetTag.ChangeEquip;
            try
            {
                C2S_ChangeEquip c2s = JsonConvert.DeserializeObject<C2S_ChangeEquip>(data);

                UserInfo.getUserInfoData(c2s.DeviceId).EquipList = c2s.EquipList;
                UserInfo.getUserInfoData(c2s.DeviceId).BagList = c2s.BagList;

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
