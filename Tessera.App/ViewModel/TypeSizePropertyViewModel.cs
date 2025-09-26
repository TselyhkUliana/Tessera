using MappingManager.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera.PolinomProvider;

namespace Tessera.App.ViewModel
{
  public class TypeSizePropertyViewModel : ViewModelBase
  {
    private string _name;
    private string _value;
    private PropertyType _type;    

    public string Name { get => _name; set => Set(ref _name, value); }
    public string Value { get => _value; set => Set(ref _value, value); }
    public PropertyType Type { get => _type; set => Set(ref _type, value); }
  }
}