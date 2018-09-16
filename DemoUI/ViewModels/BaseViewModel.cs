using PropertyChanged;
using System.ComponentModel;

namespace DemoUI.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender,e)=> { };
    }
}
