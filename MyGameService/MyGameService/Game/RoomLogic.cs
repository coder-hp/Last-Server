using SocketUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGameService.Game
{
    public class RoomUserInfo
    {
        public ClientInfo clientInfo;
        public int userId;

        public RoomUserInfo(ClientInfo _clientInfo, int _userId)
        {
            clientInfo = _clientInfo;
            userId = _userId;
        }
    }

    public class RoomLogic
    {
        public List<RoomUserInfo> list_user = new List<RoomUserInfo>();

        public RoomLogic(List<WaitMatchUserInfo> userList)
        {
            for(int i = 0; i < userList.Count; i++)
            {
                list_user.Add(new RoomUserInfo(userList[i].clientInfo, userList[i].userId));
            }

            RoomManager.addRoom(this);
        }

        public bool checkIsExistUser(int userId)
        {
            for(int i = 0; i < list_user.Count; i++)
            {
                if(list_user[i].userId == userId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool checkIsExistUser(ClientInfo clientInfo)
        {
            for (int i = 0; i < list_user.Count; i++)
            {
                if (list_user[i].clientInfo == clientInfo)
                {
                    return true;
                }
            }

            return false;
        }

        public void deleteUser(int userId)
        {
            for (int i = 0; i < list_user.Count; i++)
            {
                if (list_user[i].userId == userId)
                {
                    list_user.RemoveAt(i);
                    break;
                }
            }
        }

        public void deleteUser(ClientInfo clientInfo)
        {
            for (int i = 0; i < list_user.Count; i++)
            {
                if (list_user[i].clientInfo == clientInfo)
                {
                    list_user.RemoveAt(i);
                    break;
                }
            }
        }

        public void broadcastState(C2S_SubmitState c2s)
        {
            S2C_BroadcastState s2c = new S2C_BroadcastState();
            s2c.Tag = CSParam.NetTag.BroadcastState.ToString();
            s2c.Code = (int)CSParam.CodeType.Ok;
            s2c.UserId = c2s.UserId;
            s2c.action = c2s.action;
            s2c.pos_x = c2s.pos_x;
            s2c.pos_y = c2s.pos_y;
            s2c.pos_z = c2s.pos_z;
            s2c.rotate_y = c2s.rotate_y;

            for (int i = 0; i < list_user.Count; i++)
            {
                if (list_user[i].userId != c2s.UserId)
                {
                    Socket_S.getInstance().Send(list_user[i].clientInfo, s2c);
                }
            }
        }
    }
}
