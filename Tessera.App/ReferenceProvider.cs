using Ascon.Polynom.Api;
using Ascon.Polynom.Login;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tessera.App.ViewModel;

namespace Tessera.App
{
  internal class ReferenceProvider
  {
    private static ReferenceProvider _instance;
    private ISession _sesion;


    private ReferenceProvider()
    {
      LoginManager.TryOpenSession(Guid.Parse(Constants.POLYNOM_CLIENT_ID), out _sesion);
    }

    public static ReferenceProvider Instance => _instance ??= new ReferenceProvider();

    public void Find(IList<SectionDefinitionViewModel> sectionDefinitionViewModels)
    {
      var transaction = _sesion.Objects.StartTransaction();
      var sectionDefinition = sectionDefinitionViewModels.First();
      var material = CreateOrReceiveMaterial(sectionDefinition);
      var sortament = CreateOrReceiveSortament(sectionDefinition);
      transaction.Commit();
    }

    private IElement CreateOrReceiveSortament(SectionDefinitionViewModel sectionDefinition)
    {
      var inputSortament = sectionDefinition.SectionProfile;
      var similarSortament = sectionDefinition.SuggestedProfiles.First();
      var reference = _sesion.Objects.AllReferences.FirstOrDefault(r => r.Name == Constants.REFENCE_NAME);
      var catalog = reference.Catalogs.FirstOrDefault(c => c.Name == Constants.CATALOG_SORTAMENT);
      var concept = _sesion.Objects.GetKnownConcept(KnownConceptKind.Element);
      var propDef = _sesion.Objects.GetKnownPropertyDefinition(KnownPropertyDefinitionKind.Name);
      var simpleCondition = _sesion.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(similarSortament));
      var searchElement = Search(similarSortament, Constants.CATALOG_SORTAMENT);

      if (similarSortament.Equals(inputSortament, StringComparison.OrdinalIgnoreCase))
        return searchElement;

      var inputElementFormat = inputSortament.FirstCharToUpper();
      var inputElementWords = inputElementFormat.Split(' ');
      var simularElementWords = similarSortament.Split(' ');
      var inputElementWordFirst = inputElementWords[0];
      var group = inputElementWordFirst == simularElementWords[0]
        ? searchElement.OwnerGroup
        : catalog.Groups.First().CreateGroup(inputElementWordFirst.FirstCharToUpper());

      var element = group.CreateElement(inputElementFormat);
      element.Applicability = Applicability.Allowed;
      var markProperty = element.GetProperty(Constants.PROP_SORTAMENT_MASK).Definition as IStringPropertyDefinition;
      var conceptSortament = _sesion.Objects.Get<IConcept>(Constants.CONCEPT_SORTAMENT);
      markProperty.AssignStringPropertyValue(element, conceptSortament, ExtractName(inputElementFormat));

      return element;
    }

    private IElement CreateOrReceiveMaterial(SectionDefinitionViewModel sectionDefinition)
    {
      var inputMaterial = sectionDefinition.Material;
      var similarMaterial = sectionDefinition.SuggestedMaterials.First();
      var searchElement = Search(similarMaterial, Constants.CATALOG_MATERIAL);

      if (similarMaterial.Equals(inputMaterial, StringComparison.OrdinalIgnoreCase))
        return searchElement;

      var inputElementFormat = inputMaterial.FirstCharToUpper();
      var group = searchElement.OwnerGroup;
      var element = group.CreateElement(inputElementFormat);
      element.Applicability = Applicability.Allowed;
      var markProperty = element.GetProperty(Constants.PROP_MATERIAL_MASK).Definition as IStringPropertyDefinition;
      var conceptMaterial = _sesion.Objects.Get<IConcept>(Constants.CONCEPT_MATERIAL);
      markProperty.AssignStringPropertyValue(element, conceptMaterial, ExtractName(inputElementFormat));

      return element;
    }

    private IElement? Search(string similarElement, string catalogName)
    {
      var reference = _sesion.Objects.AllReferences.FirstOrDefault(r => r.Name == Constants.REFENCE_NAME);
      var catalog = reference.Catalogs.FirstOrDefault(c => c.Name == catalogName);
      var concept = _sesion.Objects.GetKnownConcept(KnownConceptKind.Element);
      var propDef = _sesion.Objects.GetKnownPropertyDefinition(KnownPropertyDefinitionKind.Name);
      var condition = _sesion.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(similarElement));
      var searchElement = catalog.Intersect(condition).GetEnumerable<IElement>().FirstOrDefault();
      return searchElement;
    }

    private static string ExtractName(string fullName)
    {
      var index = Constants.Standards
        .Select(x => fullName.IndexOf(x, StringComparison.OrdinalIgnoreCase))
        .Where(x => x > 0)
        .FirstOrDefault();

      return index > 0 ? fullName.Substring(0, index).Trim() : fullName.Trim();
    }
  }
}
//TODO: Могут цифры не идти сразу после госта ТБ 103/70 ГОСТ Р 70235-2022 
//TODO: для экземпляров нужно если не совпадает гост
//TODO: может быть не Intersect
//TODO: Разобраться с регистром
//TODO: Заполнять для сортаментов форма и профиль загаотовки вид заготовки код типа профиля (подумать над этим)