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
      var sectionDefinition = sectionDefinitionViewModels.First();
      //Material(sectionDefinition);
      Sortament(sectionDefinition);
    }

    private void Sortament(SectionDefinitionViewModel sectionDefinition)
    {
      var transaction = _sesion.Objects.StartTransaction();
      var inputElementSortament = sectionDefinition.SectionProfile;
      var simularElementSortament = sectionDefinition.SuggestedProfiles.First();
      var reference = _sesion.Objects.AllReferences.FirstOrDefault(r => r.Name == Constants.REFENCE_NAME);
      var catalog = reference.Catalogs.FirstOrDefault(c => c.Name == Constants.CATALOG_SORTAMENT);
      var concept = _sesion.Objects.GetKnownConcept(KnownConceptKind.Element);
      var propDef = _sesion.Objects.GetKnownPropertyDefinition(KnownPropertyDefinitionKind.Name);
      var condition = _sesion.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(inputElementSortament));

      var resultScope = catalog.Intersect(condition);
      var element = resultScope.GetEnumerable<IElement>().FirstOrDefault();
      if (element is null)
      {
        var inputWords = inputElementSortament.Split(' ');
        var simularWords = simularElementSortament.Split(' ');

        var condition2 = _sesion.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(simularElementSortament));
        var resultScope2 = catalog.Intersect(condition2);
        var element2 = resultScope2.GetEnumerable<IElement>().FirstOrDefault();

        var inputWordFirst = inputWords[0];
        if (inputWordFirst == simularWords[0])
        {
          var parentGroup = element2.OwnerGroup;
          var element3 = parentGroup.CreateElement(inputElementSortament);
          element3.Applicability = Applicability.Allowed;
          var p = element3.GetProperty(Constants.PROP_MATERIAL_MASK);
          var prop = element3.GetProperty(Constants.PROP_MATERIAL_MASK).Definition as IStringPropertyDefinition;
          var concept2 = _sesion.Objects.Get<IConcept>(Constants.CONCEPT_SORTAMENT);
          var index = Constants.Standarts.Select(x => inputElementSortament.IndexOf(x, StringComparison.OrdinalIgnoreCase)).Where(x => x != -1).FirstOrDefault();
          prop.AssignStringPropertyValue(element3, concept2, inputElementSortament.Substring(0, index));
        }
        else
        {
          var parentGroup = catalog.Groups.First().CreateGroup(char.ToUpper(inputWordFirst[0]) + inputWordFirst.Substring(1));
          var element3 = parentGroup.CreateElement(inputElementSortament);
          element3.Applicability = Applicability.Allowed;
          var prop = element3.GetProperty(Constants.PROP_MATERIAL_MASK).Definition as IStringPropertyDefinition;
          var concept2 = _sesion.Objects.Get<IConcept>(Constants.CONCEPT_SORTAMENT);
          var index = Constants.Standarts.Select(x => inputElementSortament.IndexOf(x, StringComparison.OrdinalIgnoreCase)).Where(x => x != -1).FirstOrDefault();
          prop.AssignStringPropertyValue(element3, concept2, inputElementSortament.Substring(0, index));
        }
      }
      transaction.Commit();
    }

    private void Material(SectionDefinitionViewModel sectionDefinition)
    {
      var inputElementMaterial = sectionDefinition.Material;
      var simularElementMaterial = sectionDefinition.SuggestedMaterials.First();

      var reference = _sesion.Objects.AllReferences.FirstOrDefault(r => r.Name == Constants.REFENCE_NAME);
      var catalog = reference.Catalogs.FirstOrDefault(c => c.Name == Constants.CATALOG_MATERIAL);
      var concept = _sesion.Objects.GetKnownConcept(KnownConceptKind.Element);
      var propDef = _sesion.Objects.GetKnownPropertyDefinition(KnownPropertyDefinitionKind.Name);
      var condition = _sesion.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(inputElementMaterial));

      var resultScope = catalog.Intersect(condition);
      var element = resultScope.GetEnumerable<IElement>().FirstOrDefault();
      if (element is null)
      {
        var condition2 = _sesion.Objects.CreateSimpleCondition(concept, propDef, (int)StringCompareOperation.Equal, ((IStringPropertyDefinition)propDef).CreateStringPropertyValueData(simularElementMaterial));
        var resultScope2 = catalog.Intersect(condition2);
        var element2 = resultScope2.GetEnumerable<IElement>().FirstOrDefault();

        if (inputElementMaterial.Contains("ГОСТ") && simularElementMaterial.Contains("ГОСТ"))
        {
          var inputElementWords = inputElementMaterial.Split(' ');
          var simularElementWords = simularElementMaterial.Split(' ');
          var inputElementGOST = inputElementWords[inputElementMaterial.IndexOf("ГОСТ") + 1];
          var simularElementGOST = simularElementWords[simularElementMaterial.IndexOf("ГОСТ") + 1];
          if (inputElementGOST == simularElementGOST)
          {
            var group = element2.OwnerGroup;
            group.CreateElement(inputElementMaterial);
          }
        }
        else
        {
          var group = element2.OwnerGroup;
          group.CreateElement(inputElementMaterial);
        }
      }
    }
  }
}
//TODO: Может быть не гост а ту или другой стандарт
//TODO: Могут цифры не идти сразу после госта ТБ 103/70 ГОСТ Р 70235-2022 
//TODO: наверное не нужно делать новые группы для материалов (ложить туда где есть похожий)
//TODO: наверное не нужно делать новые группы для сортаментов (ложить туда где есть похожий) (подумать о необычных случиях например восьмигранник (как вариант брать первое слово для названия группы))
//TODO: для экземпляров нужно если не совпадает гост
//TODO: может быть не Intersect
//TODO: Разобраться с регистром
//TODO: Заполнять для сортаментов свойство марка
//TODO: Заполнять для сортаментов форма и профиль загаотовки вид заготовки код типа профиля