using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class User_Preset : DBTablePreset
{
    public User_Preset(string name)
    {
        tableName = name;
    }

    public override void initKey()
    {
        keyList.Add(new TableKeyObj("id", TableKeyObj.ValueType.ValueType_string));
        keyList.Add(new TableKeyObj("account", TableKeyObj.ValueType.ValueType_string));
        keyList.Add(new TableKeyObj("password", TableKeyObj.ValueType.ValueType_string));
        keyList.Add(new TableKeyObj("phone", TableKeyObj.ValueType.ValueType_string));
        keyList.Add(new TableKeyObj("zhanshilevel", TableKeyObj.ValueType.ValueType_int));
        keyList.Add(new TableKeyObj("fashilevel", TableKeyObj.ValueType.ValueType_int));
        keyList.Add(new TableKeyObj("huanshilevel", TableKeyObj.ValueType.ValueType_int));
        keyList.Add(new TableKeyObj("createtime", TableKeyObj.ValueType.ValueType_string));
    }
}
