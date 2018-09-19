﻿using PropertyChanged;
using System.ComponentModel;

namespace DemoUI
{
    [AddINotifyPropertyChangedInterface]
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender,e)=> { };
    }
}
