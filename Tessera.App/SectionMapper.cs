using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera.App.ViewModel;
using Tessera.PolinomProvider.Model;

namespace Tessera.App
{
  internal static class SectionMapper
  {
    public static SectionDefinition ToDomain(SectionDefinitionViewModel vm)
    {
      return new SectionDefinition
      {
        Material = vm.Material,
        Sortament = vm.Sortament,
        TypeSize = vm.TypeSizeViewModel.TypeSize,
        SortamentEx = vm.SortamentEx,
        SuggestedMaterials = vm.SuggestedMaterials.ToList(),
        SuggestedSortament = vm.SuggestedSortament.ToList(),
        TypeSizeData = new TypeSizeData
        {
          Properties = vm.TypeSizeViewModel.TypeSizePropertiesViewModel.Select(p => new TypeSizeProperty
          {
            Name = p.Name,
            Value = p.Value,
            Type = p.Type
          }).ToList()
        }
      };
    }
  }
}