using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGameService.Game
{
    class RoomManager
    {
        static List<RoomLogic> list_room = new List<RoomLogic>();

        public static void addRoom(RoomLogic roomLogic)
        {
            list_room.Add(roomLogic);
        }

        public static RoomLogic getRoomByUserId(int userId)
        {
            for (int i = 0; i < list_room.Count; i++)
            {
                if (list_room[i].checkIsExistUser(userId))
                {
                    return list_room[i];
                }
            }

            return null;
        }
    }
}
