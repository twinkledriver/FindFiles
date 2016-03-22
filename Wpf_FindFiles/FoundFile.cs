using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf_FindFiles
{
    public class FoundFile :INotifyPropertyChanged
    {
        private string _name;
        public string Name
        {
            get
            {
                return _name;    //m_name表示是类里的成员变量，作用域是整个类。name是局部变量，作用域是其所在的函数里。这是编程里约定俗成的写法，便于区分和以后的代码维护，是编程规范的一部分。
            }
            set                     //而封装的体现就是类的成员变量都是私有的，外部不能直接给成员变量赋值，通过一个函数接口，如Set函数
            {
                if (_name != value)
                {
                    OnPropertyChanged("Name");      //OnPropertyChanged需要你在属性值每次变化的时候主动调用一个方法，会引发此事件，当Entity绑定到控件时，控件会主动注册OnPropertyChanged事件，所以属性变化的时候控件会自动更新
                    _name = value;
                }
            }
        }
        private long _length;
        public long Length
        {
            get
            {
                return _length;
            }
            set
            {
                if(_length!=value)
                {
                    OnPropertyChanged("Length");
                    _length=value;
                }
            }
        }

        private string _location;
        public string Location
        {
            get
            {
                return _location;            
            }
            set
            {
                if (_location != value)
                {
                    OnPropertyChanged("Location");
                    _location = value;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));      //Handler一般用于程序加载处理，如建立缓存机制、程序初始化..,Handler和Module的区别在于，Handler执行完后是不会返回其他任何页面的，而Module则会继续执行后面的操作
            }
        }
    }
}











