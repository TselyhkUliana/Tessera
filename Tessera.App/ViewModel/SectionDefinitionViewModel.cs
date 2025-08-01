using MappingManager.ViewModel.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera.App.Model;

namespace Tessera.App.ViewModel
{
  internal class SectionDefinitionViewModel : ViewModelBase
  {
    private readonly SectionDefinition _sectionDefinition;
    private readonly ISuggestionProvider _suggestionProvider;

    public SectionDefinitionViewModel(SectionDefinition sectionDefinition, ISuggestionProvider suggestionProvider)
    {
      _sectionDefinition = sectionDefinition;
      _suggestionProvider = suggestionProvider;
    }

    public string Material
    {
      get => _sectionDefinition.Material;
      set
      {
        if (string.IsNullOrEmpty(value) || value == _sectionDefinition.Material)
          return;

        if (_suggestionProvider.SuggestedMaterials.Contains(value))
        {
          Set(v => _sectionDefinition.Material = v, _sectionDefinition.Material, value);
          return;
        }

        if (Set((v) => _sectionDefinition.Material = v, _sectionDefinition.Material, value))
          _ = _suggestionProvider.UpdateSuggestionsAsync(value, _suggestionProvider.SuggestedMaterials, _suggestionProvider.MaterialEmbeddings);
      }
    }

    public string SectionProfile
    {
      get => _sectionDefinition.SectionProfile;
      set
      {
        if (string.IsNullOrEmpty(value) || value == _sectionDefinition.SectionProfile)
          return;

        if (_suggestionProvider.SuggestedProfiles.Contains(value))
        {
          Set((v) => _sectionDefinition.SectionProfile = v, _sectionDefinition.SectionProfile, value);
          return;
        }

        if (Set((v) => _sectionDefinition.SectionProfile = v, _sectionDefinition.SectionProfile, value))
          _ = _suggestionProvider.UpdateSuggestionsAsync(value, _suggestionProvider.SuggestedProfiles, _suggestionProvider.ProfileEmbeddings);
      }
    }

    public string SectionInstance
    {
      get => _sectionDefinition.SectionInstance;
      set
      {
        if (string.IsNullOrEmpty(value) || value == _sectionDefinition.SectionInstance)
          return;

        if (_suggestionProvider.SuggestedInstances.Contains(value))
        {
          Set((v) => _sectionDefinition.SectionInstance = v, _sectionDefinition.SectionInstance, value);
          return;
        }

        if (Set((v) => _sectionDefinition.SectionInstance = v, _sectionDefinition.SectionInstance, value))
          _ = _suggestionProvider.UpdateSuggestionsAsync(value, _suggestionProvider.SuggestedInstances, _suggestionProvider.InstanceEmbeddings);
      }
    }

    public SectionDefinition Model => _sectionDefinition;
  }
}
