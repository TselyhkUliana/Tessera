using MappingManager.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera.PolinomProvider.Model;

namespace Tessera.App.ViewModel
{
  [DebuggerDisplay("SectionDefinitionViewModel: {Material} - {Sortament} - {TypeSizeViewModel.TypeSize}")]
  public class SectionDefinitionViewModel : ViewModelBase
  {
    private readonly SectionDefinition _sectionDefinition;
    private readonly ISuggestionProvider _suggestionProvider;
    private readonly TypeSizeViewModel _typeSizeViewModel;
    private bool _isUpdatingSuggestions;
    private bool _isInternalChange = false;

    public SectionDefinitionViewModel(SectionDefinition sectionDefinition, ISuggestionProvider suggestionProvider)
    {
      _sectionDefinition = sectionDefinition;
      _suggestionProvider = suggestionProvider;
      _typeSizeViewModel = new TypeSizeViewModel(this);
    }

    public event EventHandler RequestAddSectionDefinition;
    public event EventHandler SortamentEditFinished;
    public SectionDefinition Model => _sectionDefinition;
    /// <summary>Материал</summary>
    public string Material
    {
      get => _sectionDefinition.Material;
      set
      {
        if (_isInternalChange)
          return;

        if (!Set(v => _sectionDefinition.Material = v, _sectionDefinition.Material, value))
          return;

        if (!_isUpdatingSuggestions)
        {
          _isUpdatingSuggestions = true;
          OnRequestAddSectionDefinition();
        }
        _ = UpdateSuggestionsSafeAsync(value, SuggestedMaterials, _suggestionProvider.MaterialEmbeddings);
      }
    }
    /// <summary>Сортамент</summary>
    public string Sortament
    {
      get => _sectionDefinition.Sortament;
      set
      {
        if (_isInternalChange)
          return;

        if (!Set((v) => _sectionDefinition.Sortament = v, _sectionDefinition.Sortament, value))
          return;

        if (!_isUpdatingSuggestions)
        {
          OnRequestAddSectionDefinition();
          _isUpdatingSuggestions = true;
        }
        _ = UpdateSuggestionsSafeAsync(value, SuggestedSortament, _suggestionProvider.SortamentEmbeddings);
      }
    }
    /// <summary>Экземпляр сортамента</summary>
    public string SortamentEx
    {
      get => _sectionDefinition.SortamentEx;
      set
      {
        if (_isInternalChange)
          return;

        if (!Set((v) => _sectionDefinition.SortamentEx = v, _sectionDefinition.SortamentEx, value))
          return;

        if (!_isUpdatingSuggestions)
        {
          OnRequestAddSectionDefinition();
          _isUpdatingSuggestions = true;
        }
        _ = UpdateSuggestionsSafeAsync(value, SuggestedSortamentEx, _suggestionProvider.SortamentExEmbeddings);
      }
    }
    /// <summary>ViewModel Типоразмера</summary>
    public TypeSizeViewModel TypeSizeViewModel => _typeSizeViewModel;

    /// <summary>Список похожих материалов</summary>
    public ObservableCollection<string> SuggestedMaterials { get; set; }
    /// <summary>Список похожих сортаментов</summary>
    public ObservableCollection<string> SuggestedSortament { get; set; }
    /// <summary>Список похожих экземпляров сортаментов</summary>
    public ObservableCollection<string> SuggestedSortamentEx { get; set; }

    public void OnRequestAddSectionDefinition() => RequestAddSectionDefinition?.Invoke(this, EventArgs.Empty);

    public void FinishEditSortament()
    {
      //if (!string.IsNullOrEmpty(Sortament))
        SortamentEditFinished?.Invoke(this, EventArgs.Empty);
    }

    private async Task UpdateSuggestionsSafeAsync(string value, ObservableCollection<string> suggestionsTarget, List<(float[] Id, string Name)> embeddingDatabase)
    {
      _isInternalChange = true;
      await _suggestionProvider.UpdateSuggestionsAsync(value, suggestionsTarget, embeddingDatabase);
      _isInternalChange = false;
    }
  }
}