using System.Collections;
using System.Collections.Generic;

public class CSParam
{
    public enum NetTag
    {
        Login,
        UserInfo,
        Bag,
        ChangeEquip,
        Sign,
    }
}

public class UserInfoData
{
    public string Id;
    public int Gold;
    public List<int> BagList = new List<int>();
    public List<int> EquipList = new List<int>();
}

public class CSBaseData
{
    public int Tag;
    public int Code;
    public string Message;
}

public class C2S_Login : CSBaseData
{
    public string DeviceId;
}

public class S2C_Login : CSBaseData
{
    public string Id;
}

public class C2S_UserInfo : CSBaseData
{
    public string DeviceId;
}

public class S2C_UserInfo : CSBaseData
{
    public UserInfoData UserInfoData;
}

public class C2S_ChangeEquip : CSBaseData
{
    public string DeviceId;
    public List<int> BagList = new List<int>();
    public List<int> EquipList = new List<int>();
}

public class S2C_ChangeEquip : CSBaseData
{
}

public class C2S_Sign : CSBaseData
{
    public string Id;
}

public class S2C_Sign : CSBaseData
{
    public string Reward;
}
