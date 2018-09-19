using PropertyChanged;
using System.ComponentModel;

namespace UserControlLibrary.ControlViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender,e)=> { };
    }
}
