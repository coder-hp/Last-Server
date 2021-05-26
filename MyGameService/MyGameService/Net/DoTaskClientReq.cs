using MyGameService.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketUtil
{
    class DoTaskClientReq
    {
        public static void Do(ClientInfo clientInfo, string data)
        {
            try
            {
                C2SBaseData c2sBaseData = JsonConvert.DeserializeObject<C2SBaseData>(data);
                
                if(c2sBaseData.Tag == CSParam.NetTag.Login.ToString())
                {
                    DoTask_Login.Do(clientInfo, data);
                }
                else if (c2sBaseData.Tag == CSParam.NetTag.Register.ToString())
                {
                    DoTask_Register.Do(clientInfo, data);
                }
                else if (c2sBaseData.Tag == CSParam.NetTag.Bag.ToString())
                {
                    DoTask_Bag.Do(clientInfo, data);
                }
                else if (c2sBaseData.Tag == CSParam.NetTag.EnterGameMode2.ToString())
                {
                    DoTask_EnterGameMode2.Do(clientInfo, data);
                }
                else if (c2sBaseData.Tag == CSParam.NetTag.UserReady.ToString())
                {
                    DoTask_UserReady.Do(clientInfo, data);
                }
                else if (c2sBaseData.Tag == CSParam.NetTag.GetUserState.ToString())
                {
                    DoTask_GetUserState.Do(clientInfo, data);
                }
                else if (c2sBaseData.Tag == CSParam.NetTag.SubmitState.ToString())
                {
                    DoTask_SubmitState.Do(clientInfo, data);
                }
                else
                {
                    CommonUtil.Log("未知tag，不予处理：" + data);
                }
            }
            catch (Exception ex)
            {
                CommonUtil.Log("客户端传的数据有问题：" + data);
            }
        }
    }
}
