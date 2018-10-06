using System;
using System.Windows.Input;

namespace VisionWorkshop.Command
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged = (sender, e)=> { };
        public Action _action { get; set; }
        public Func<bool> _func { get; set; }

        public RelayCommand(Action action, Func<bool> func)
        {
            this._action = action;
            this._func = func;
        }

        public bool CanExecute(object parameter)
        {
            return _func();
        }

        public void Execute(object parameter)
        {
            _action();
        }
    }
}
