using MappingManager.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera.App.Model;

namespace Tessera.App.ViewModel
{
  [DebuggerDisplay("SectionDefinitionViewModel: {Material} - {Sortament} - {TypeSize}")]
  public class SectionDefinitionViewModel : ViewModelBase
  {
    private readonly SectionDefinition _sectionDefinition;
    private readonly ISuggestionProvider _suggestionProvider;
    public event Action RequestAddSectionDefinition;
    private bool _isUpdatingSuggestions;
    private bool _isInternalChange = false;

    public SectionDefinitionViewModel(SectionDefinition sectionDefinition, ISuggestionProvider suggestionProvider)
    {
      _sectionDefinition = sectionDefinition;
      _suggestionProvider = suggestionProvider;
    }

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
          RequestAddSectionDefinition?.Invoke();
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
          RequestAddSectionDefinition?.Invoke();
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
          RequestAddSectionDefinition?.Invoke();
          _isUpdatingSuggestions = true;
        }
        _ = UpdateSuggestionsSafeAsync(value, SuggestedSortamentEx, _suggestionProvider.SortamentExEmbeddings);
      }
    }
    /// <summary>Типоразмер</summary>
    public string TypeSize { get => _sectionDefinition.TypeSize; set => Set((v) => _sectionDefinition.TypeSize = v, _sectionDefinition.TypeSize, value); }

    /// <summary>Список похожих материалов</summary>
    public ObservableCollection<string> SuggestedMaterials { get; set; }
    /// <summary>Список похожих сортаментов</summary>
    public ObservableCollection<string> SuggestedSortament { get; set; }
    /// <summary>Список похожих экземпляров сортаментов</summary>
    public ObservableCollection<string> SuggestedSortamentEx { get; set; }

    public SectionDefinition Model => _sectionDefinition;

    private async Task UpdateSuggestionsSafeAsync(string value, ObservableCollection<string> suggestionsTarget, List<(float[] Id, string Name)> embeddingDatabase)
    {
      _isInternalChange = true;
      await _suggestionProvider.UpdateSuggestionsAsync(value, suggestionsTarget, embeddingDatabase);
      _isInternalChange = false;
    }
  }
}