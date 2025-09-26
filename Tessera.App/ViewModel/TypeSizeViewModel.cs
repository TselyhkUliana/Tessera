using MappingManager.ViewModel.Base;
using System.Collections.ObjectModel;

namespace Tessera.App.ViewModel
{
  public class TypeSizeViewModel : ViewModelBase
  {
    private readonly SectionDefinitionViewModel _sectionDefinitionViewModel;
    private ObservableCollection<TypeSizePropertyViewModel> _properties;
    private bool _isTypeSizeEditing;

    public TypeSizeViewModel(SectionDefinitionViewModel sectionDefinitionViewModel)
    {
      _sectionDefinitionViewModel = sectionDefinitionViewModel;
      _properties = new ObservableCollection<TypeSizePropertyViewModel>(PolinomProvider.PolinomProvider.Instance.AllDimensionProperties.Select(x => new TypeSizePropertyViewModel
      {
        Name = x.Key,
        Type = x.Value,
        Value = string.Empty
      }));
      _sectionDefinitionViewModel.SortamentEditFinished += SectionDefinitionSortamentEditFinished;
    }

    /// <summary>Типоразмер</summary>
    public string TypeSize
    {
      get => _sectionDefinitionViewModel.Model.TypeSize;
      set
      {
        if (!Set((v) => _sectionDefinitionViewModel.Model.TypeSize = v, _sectionDefinitionViewModel.Model.TypeSize, value))
          return;

        _sectionDefinitionViewModel.OnRequestAddSectionDefinition();
      }
    }
    public bool IsTypeSizeEditing
    {
      get => _isTypeSizeEditing;
      set
      {
        if (_isTypeSizeEditing != value)
        {
          _isTypeSizeEditing = value;
          OnPropertyChanged();
        }
      }
    }
    public ObservableCollection<TypeSizePropertyViewModel> TypeSizePropertiesViewModel { get => _properties; set => Set(ref _properties, value); }

    private void SectionDefinitionSortamentEditFinished(object sender, EventArgs e)
    {
      if (string.IsNullOrEmpty(_sectionDefinitionViewModel.Sortament))
      {
        _properties = new ObservableCollection<TypeSizePropertyViewModel>(_properties.OrderBy(x => x.Name)); //потом попробовать https://stackoverflow.com/questions/7284805/how-to-sort-observablecollection
        OnPropertyChanged(nameof(TypeSizePropertiesViewModel));
        return;
      }

      var properties = PolinomProvider.PolinomProvider.Instance.GetPropertiesForTypeSize(_sectionDefinitionViewModel.SuggestedSortament.First());
      foreach (var item in properties)
      {
        var propertyViewModel = _properties.FirstOrDefault(x => x.Name == item);
        if (propertyViewModel is not null)
          _properties.Move(_properties.IndexOf(propertyViewModel), 0);
      }
      OnPropertyChanged(nameof(TypeSizePropertiesViewModel));
    }
  }
}