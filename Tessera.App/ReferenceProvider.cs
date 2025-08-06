using Ascon.Polynom.Api;
using Ascon.Polynom.Login;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Tessera.App.ViewModel;

namespace Tessera.App
{
  internal class ReferenceProvider
  {
    private static ReferenceProvider _instance;
    private ISession _sesion;
    private IReference? _reference;

    private ReferenceProvider()
    {
      LoginManager.TryOpenSession(Guid.Parse(Constants.POLYNOM_CLIENT_ID), out _sesion);
      _reference = _sesion.Objects.AllReferences.FirstOrDefault(x => x.Name == Constants.REFENCE_NAME);
    }

    public static ReferenceProvider Instance => _instance ??= new ReferenceProvider();

    public void Find(IList<SectionDefinitionViewModel> sectionDefinitionViewModels)
    {
      var transaction = _sesion.Objects.StartTransaction();
      var sectionDefinition = sectionDefinitionViewModels.First();
      var material = CreateOrReceiveMaterial(sectionDefinition);
      var sortament = CreateOrReceiveSortament(sectionDefinition);
      var typeSize = CreateOrReceiveTypeSize(sectionDefinition, sortament.Name);
      var sortamentEx = CreateSortamentEx(sortament);
      CreateLink(sortament, material, Constants.LINK_SORTAMENT_MATERIAL);
      CreateLink(typeSize, sortament, Constants.LINK_TYPESIZE_SORTAMENT);
      CreateLink(sortamentEx, sortament, Constants.LINK_SORTAMENTEX_SORTAMENT);
      CreateLink(sortamentEx, material, Constants.LINK_SORTAMENTEX_MATERIAL);
      CreateLink(sortamentEx, typeSize, Constants.LINK_SORTAMENTEX_TYPE_SIZE);
      transaction.Commit();
      LinksTest(sortament);
    }

    public IElement CreateSortamentEx(IElement sortament)
    {
      var catalog = _reference.Catalogs.FirstOrDefault(x => x.Name == Constants.CATALOG_SORTAMENT_EX);
      var baseName = EntityNameHelper.GetNameBeforeStandard(sortament.Name);
      var index = EntityNameHelper.GetStandardKeywordIndex(sortament.Name);
      var standardPart = sortament.Name.Substring(index);
      var groupName = $"{baseName} {standardPart} {standardPart}"; //такой формат названия групп (например: Анод (золотой) ГОСТ 25475-2015 ГОСТ 25475-2015))
      var group = GetGroup(catalog.Groups, groupName) ?? CreateGroupSortamentEx(groupName, sortament.OwnerGroup.Name);
      var element = group.CreateElement("тест");
      element.Applicability = Applicability.Allowed;
      return element;
    }

    public IElement CreateOrReceiveTypeSize(SectionDefinitionViewModel sectionDefinition, string groupName)
    {
      var catalog = _reference.Catalogs.FirstOrDefault(x => x.Name == Constants.CATALOG_TYPE_SIZE);
      var group = GetGroup(catalog.Groups, groupName) ?? catalog.CreateGroup(groupName);
      var element = group.Elements.Where(e => e.Name.Equals(sectionDefinition.TypeSize, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      element ??= group.CreateElement(sectionDefinition.TypeSize);
      element.Applicability = Applicability.Allowed;
      return element;
    }

    private IElement CreateOrReceiveSortament(SectionDefinitionViewModel sectionDefinition)
    {
      var inputSortament = sectionDefinition.SectionProfile.Trim();
      var similarSortament = sectionDefinition.SuggestedProfiles.First().Trim();
      var catalog = _reference.Catalogs.FirstOrDefault(c => c.Name == Constants.CATALOG_SORTAMENT);
      var concept = _sesion.Objects.GetKnownConcept(KnownConceptKind.Element);
      var propDef = _sesion.Objects.GetKnownPropertyDefinition(KnownPropertyDefinitionKind.Name);
      var simpleCondition = _sesion.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(similarSortament));
      var searchElement = SearchElement(similarSortament, Constants.CATALOG_SORTAMENT);

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
      markProperty.AssignStringPropertyValue(element, conceptSortament, EntityNameHelper.GetNameBeforeStandard(inputElementFormat));
      return element;
    }

    private IElement CreateOrReceiveMaterial(SectionDefinitionViewModel sectionDefinition)
    {
      var inputMaterial = sectionDefinition.Material;
      var similarMaterial = sectionDefinition.SuggestedMaterials.First();
      var searchElement = SearchElement(similarMaterial, Constants.CATALOG_MATERIAL);

      if (similarMaterial.Equals(inputMaterial, StringComparison.OrdinalIgnoreCase))
        return searchElement;

      var inputElementFormat = inputMaterial.FirstCharToUpper();
      var group = searchElement.OwnerGroup;
      var element = group.CreateElement(inputElementFormat);
      element.Applicability = Applicability.Allowed;
      var markProperty = element.GetProperty(Constants.PROP_MATERIAL_MASK).Definition as IStringPropertyDefinition;
      var conceptMaterial = _sesion.Objects.Get<IConcept>(Constants.CONCEPT_MATERIAL);
      markProperty.AssignStringPropertyValue(element, conceptMaterial, EntityNameHelper.GetNameBeforeStandard(inputElementFormat));
      return element;
    }

    private IElement SearchElement(string similarElement, string catalogName)
    {
      var catalog = _reference.Catalogs.FirstOrDefault(c => c.Name == catalogName);
      var concept = _sesion.Objects.GetKnownConcept(KnownConceptKind.Element);
      var propDef = _sesion.Objects.GetKnownPropertyDefinition(KnownPropertyDefinitionKind.Name);
      var condition = _sesion.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(similarElement));
      var searchElement = catalog.Intersect(condition).GetEnumerable<IElement>().FirstOrDefault();
      return searchElement;
    }

    public void CreateLink(ILinkable left, ILinkable right, string aboluteCodeLink)
    {
      var group = _sesion.Objects.LinkDefCatalog.LinkDefGroups.FirstOrDefault(g => g.Name == Constants.REFENCE_NAME);
      var link = group.LinkDefinitions.FirstOrDefault(l => l.AbsoluteCode == aboluteCodeLink);
      link.Destination.CreateLink(left, right);
    }

    private static IGroup GetGroup(IApiReadOnlyCollection<IGroup> groups, string groupName)
    {
      foreach (var group in groups)
      {
        if (group.Name == groupName)
          return group;

        var foundGroup = GetGroup(group.Groups, groupName);
        if (foundGroup is not null)
          return foundGroup;
      }
      return null;
    }

    public IGroup CreateGroupSortamentEx(string derivedGroupName, string baseGroupName)
    {
      var catalog = _reference.Catalogs.FirstOrDefault(x => x.Name == Constants.CATALOG_SORTAMENT_EX);
      var group = catalog.Groups.FirstOrDefault(x => x.Name == baseGroupName) ?? catalog.CreateGroup(baseGroupName);
      return group.CreateGroup(derivedGroupName);
    }

    private void LinksTest(IElement element)
    {
      foreach (var link in element.Links)
      {
        Debug.WriteLine(link.Name);
        Debug.WriteLine("\tСвязанные объекты:");
        foreach (var linkedElement in link.LinkedItems)
        {
          Debug.WriteLine("\t\t" + ((IElement)linkedElement).Name + $" -  -  - {((IElement)linkedElement).ObjectId}");
        }
      }
    }
  }
}
//TODO: для экземпляров нужно если не совпадает гост
//TODO: может быть не Intersect
//TODO: Разобраться с регистром
//TODO: Заполнять для сортаментов форма и профиль загаотовки вид заготовки код типа профиля (подумать над этим)
//TODO: Подумать над заполнением понятия размерность для типоразмеров
//TODO: для экземпляров сортамента подумать над созданием группы и добавлением понятия для группы свойства по гост...