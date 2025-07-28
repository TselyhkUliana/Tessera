using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MappingManager.ViewModel.Base
{
  public class ViewModelBase : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string PropertyName = null)
    {
      if (EqualityComparer<T>.Default.Equals(field, value))
        return false;

      field = value;
      OnPropertyChanged(PropertyName);
      return true;
    }

    protected bool Set<T>(Action<T> setter, T field, T value, [CallerMemberName] string propertyName = "")
    {
      if (EqualityComparer<T>.Default.Equals(field, value))
        return false;

      setter(value);
      OnPropertyChanged(propertyName);
      return true;
    }
  }
}
