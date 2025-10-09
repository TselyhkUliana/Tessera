using Ascon.Polynom.Api;

namespace Tessera.PolinomProvider.Utils
{
  internal static class FormulaDefaults
  {
    /// <summary>Набор параметров по умолчанию: имя параметра и выражение для вычисления значения</summary>
    public static readonly (string name, string expression)[] Parameters =
    {
       ("Сортамент.Форма", "GetPropertyValue(First(GetLinkedObjects([this], 'ld:ExSortSortLinkCode::lde:Destination')), 'c:ShapesMiS::pd:ShapeMiS', '')"),
       ("Материал.Марка", "GetPropertyValue(First(GetLinkedObjects([this], 'ld:ExSortMatLinkCode::lde:Destination')), 'c:@Materials::c:Materials::pd:MaterialsMark', '')"),
       ("Типоразмер.Наименование", "GetPropertyValue(First(GetLinkedObjects([this], 'ld:ExSortSizeLinkCode::lde:Destination')), 'c:@NameAndDescription::pd:@Name', '')"),
       ("ТУ.Документ", "GetPropertyValue(First(GetDocuments([this])), 'c:@NameAndDescription::c:@Document::pd:@Designation', '')")
    };

    public static (string name, string expression) BuildParameterByDefinition(IPropertyDefinition definition, string absoluteCode)
    {
      return ($"{definition.Name}.{definition.OwnerGroup.Name}", $"GetPropertyValue([this], '{absoluteCode}', '')");
    }

    /// <summary>Возвращает тело формулы для вычисления обозначения экземпляра сортамента</summary>
    public static string BuildFormulaBodyDesignation()
    {
      return "StringTrimStart(ToString(if(IsNull([Сортамент.Форма]), '', [Сортамент.Форма])\r\n "
           + "+ StringPrefixSuffix([Материал.Марка], ' ', '')\r\n "
           + "+ StringPrefixSuffix([Типоразмер.Наименование], ' ', '')\r\n"
           + "+ StringPrefixSuffix([ТУ.Документ], ' ', '')\r\n"
           + "), StringArray(' '))";
    }

    /// <summary>Возвращает тело формулы для вычисления обозначения экземпляра сортамента в KOMPAS-3D</summary>
    public static string BuildFormulaBodyDesignationKOMPAS()
    {
      return string.Empty;
    }
  }
}
