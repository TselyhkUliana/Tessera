namespace Tessera.PolinomProvider.Formula
{
  /// <summary>
  /// Содержит части (фрагменты) формулы, используемые при построении обозначений.
  /// </summary>
  internal static class FormulaParts
  {
    /// <summary>Набор параметров по умолчанию: имя параметра и выражение для вычисления значения</summary>
    public static readonly (string name, string expression)[] Parameters =
    {
       ("Сортамент.Форма", "GetPropertyValue(First(GetLinkedObjects([this], 'ld:ExSortSortLinkCode::lde:Destination')), 'c:ShapesMiS::pd:ShapeMiS', '')"),
       ("Типоразмер.Наименование", "GetPropertyValue(First(GetLinkedObjects([this], 'ld:ExSortSizeLinkCode::lde:Destination')), 'c:@NameAndDescription::pd:@Name', '')"),
       ("Материал.Марка", "GetPropertyValue(First(GetLinkedObjects([this], 'ld:ExSortMatLinkCode::lde:Destination')), 'c:@Materials::c:Materials::pd:MaterialsMark', '')"),       
       ("ТУ.Документ", "GetPropertyValue(First(GetDocuments([this])), 'c:@NameAndDescription::c:@Document::pd:@Designation', '')")
    };

    /// <summary>Добавляет выражение Сортамент.Форма </summary>
    internal static string Sortament() => $"+ StringPrefixSuffix([Сортамент.Форма], ' ', '')\r\n";

    /// <summary> Добавляет выражение Типоразмер.Наименование./// </summary>
    internal static string TypeSize() => "+ StringPrefixSuffix([Типоразмер.Наименование], ' ', '')\r\n";

    /// <summary>Добавляет выражение Материал.Марка./// </summary>
    internal static string Material() => "+ StringPrefixSuffix([Материал.Марка], ' ', '')\r\n";

    /// <summary>Добавляет выражение ТУ.Документ.</summary>
    internal static string Standard() => "+ StringPrefixSuffix([ТУ.Документ], ' ', '')\r\n";

    /// <summary>
    /// Добавляет стандартное выражение для свойства.
    ///<br>Формирует часть строки вида + StringPrefixSuffix([Имя], '-', '').</br> 
    /// </summary>
    internal static string Expression(string parameter) => $"+ StringPrefixSuffix([{parameter}], '-', '')\r\n";

    /// <summary>Возвращает начало выражения для свойства с приоритетом 1 в категории "Basic".</summary>
    internal static string BasicPriority(string parameter) => $"StringPrefixSuffix(StringTrimStart(ToString(if(IsNull([{parameter}]), '', [{parameter}])\r\n";

    /// <summary>
    /// Добавляет выражение для свойства с приоритетом 1 в категории "Additional".
    /// Используется в конце формулы.
    /// </summary>
    internal static string AdditionalPriority(string parameter) => $"+ StringPrefixSuffix(StringTrimStart(ToString(StringPrefixSuffix([{parameter}], ' ', '')\r\n), StringArray(' '))\r\n, ' ', '')";

    /// <summary>Завершает текущий блок формулы.</summary>
    internal static string End() => "), StringArray(' '))";
  }
}
