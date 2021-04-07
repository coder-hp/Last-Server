using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class TimerUtil
{
    public static void start(float mSec,Action<object , System.Timers.ElapsedEventArgs> action)
    {
        //实例化Timer类，mSec为毫秒
        System.Timers.Timer t = new System.Timers.Timer(mSec);

        // 设置回调事件
        t.Elapsed += new System.Timers.ElapsedEventHandler(action);

        //设置是执行一次（false）还是一直执行(true)
        t.AutoReset = true;

        // 启用
        t.Enabled = true;
    }
}