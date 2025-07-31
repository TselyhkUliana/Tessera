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
        if (!Set((v) => _sectionDefinition.Material = v, _sectionDefinition.Material, value))
          return;
        _suggestionProvider.UpdateSuggestions(value, _suggestionProvider.SuggestedMaterials, _suggestionProvider.MaterialEmbeddings);
      }
    }

    public string SectionProfile
    {
      get => _sectionDefinition.SectionProfile;
      set
      {
        if (!Set((v) => _sectionDefinition.SectionProfile = v, _sectionDefinition.SectionProfile, value))
          return;
        _suggestionProvider.UpdateSuggestions(value, _suggestionProvider.SuggestedProfiles, _suggestionProvider.ProfileEmbeddings);
      }
    }

    public string SectionInstance
    {
      get => _sectionDefinition.SectionInstance;
      set
      {
        if (!Set((v) => _sectionDefinition.SectionInstance = v, _sectionDefinition.SectionInstance, value))
          return;
        _suggestionProvider.UpdateSuggestions(value, _suggestionProvider.SuggestedInstances, _suggestionProvider.InstanceEmbeddings);
      }
    }

    public SectionDefinition Model => _sectionDefinition;
  }
}
