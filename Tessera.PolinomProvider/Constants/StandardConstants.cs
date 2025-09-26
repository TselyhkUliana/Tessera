namespace Tessera.PolinomProvider.Constants
{
  public class StandardConstants
  {
    public const string GOST = "ГОСТ";
    public const string GOST_R_ISO = "ГОСТ Р ИСО";
    public const string GOST_R = "ГОСТ Р";
    public const string TU = "ТУ";
    public const string OST = "ОСТ";
    public const string STO = "СТО";
    public const string RD = "РД";
    public const string SP = "СП";
    public const string SNiP = "СНиП";
    public const string TR_TS = "ТР ТС";
    public const string TR_EAS = "ТР ЕАЭС";
    public const string PNST = "ПНСТ";
    public const string SANPIN = "СанПиН";
    public const string GN = "ГН";
    public const string MU = "МУ";
    public const string FS = "ФС";
    public const string ND = "НД";
    public const string MK = "МЭК";
    public const string MK2 = "МЭК (IEC)";
    public const string ISO = "ISO";
    public const string EN = "EN";
    public const string ARTICLE = "Артикул";
    public const string VTU = "ВТУ";
    public const string K3 = "ЗК";
    public const string INSTRUCTION = "Инструкция";
    public const string ISO2 = "ИСО";
    public const string MRTU = "МРТУ";
    public const string RST = "РСТ";
    public const string SPECIFICATION = "Спецификация";
    public const string STU = "СТУ";
    public const string TR = "ТР";
    public const string STANDARD_AFNOR = "Стандарт AFNOR";
    public const string STANDARD_ASTM = "Стандарт ASTM";
    public const string STANDARD_AS = "Стандарт AS";
    public const string STANDARD_A = "Стандарт A";
    public const string STANDARD_BDS = "Стандарт BDS";
    public const string STANDARD_BS = "Стандарт BS";
    public const string STANDARD_CSN = "Стандарт CSN";
    public const string STANDARD_DIN = "Стандарт DIN";
    public const string STANDARD_EN = "Стандарт EN";
    public const string STANDARD_GB = "Стандарт GB";
    public const string STANDARD_JIS = "Стандарт JIS";
    public const string STANDARD_MSZ = "Стандарт MSZ";
    public const string STANDARD_ONORM = "Стандарт ONORM";
    public const string STANDARD_PNH = "Стандарт PN/H";
    public const string STANDARD_STAS = "Стандарт STAS";
    public const string STANDARD_UNE = "Стандарт UNE";
    public const string STANDARD_UNI = "Стандарт UNI";
    public const string STANDARD = "Стандарт";
    public const string TU02 = "ТУ 02";
    public const string TU14 = "ТУ 14";
    public const string TU16 = "ТУ 16";
    public const string TU1 = "ТУ 1";
    public const string TU2 = "ТУ 2";
    public const string TU38 = "ТУ 38";
    public const string TU48 = "ТУ 48";
    public const string TU6 = "ТУ 6";
    public const string TU_RB = "ТУ РБ";
    public const string TU_BY = "ТУ BY";

    //Списки стандартов отсортированы по убыванию длины строки. Это необходимо для корректного определения стандарта.Например: если сначала встретится "TU", то строка "VTU" будет ошибочно распознана как "TU". Благодаря сортировке "VTU" (длина 3) будет проверяться раньше "TU" (длина 2), и метод вернёт правильный стандарт.
    public static readonly IReadOnlyList<string> Standards = new List<string> { GOST, GOST_R_ISO, GOST_R, TU, OST, STO, RD, SP, SNiP, TR_TS, TR_EAS, PNST, SANPIN, GN, MU, FS, ND, MK, MK2, ISO, EN, ARTICLE, VTU, K3, INSTRUCTION, ISO2, MRTU, RST, SPECIFICATION, STU, TR, STANDARD, STANDARD_AFNOR, STANDARD_ASTM, STANDARD_AS, STANDARD_A, STANDARD_BDS, STANDARD_BS, STANDARD_CSN, STANDARD_DIN, STANDARD_EN, STANDARD_GB, STANDARD_JIS, STANDARD_MSZ, STANDARD_ONORM, STANDARD_PNH, STANDARD_STAS, STANDARD_UNE, STANDARD_UNI }.OrderByDescending(x => x.Length).ToList();

    public static readonly IReadOnlyList<string> SpecialDocumentStandards = new List<string> { ARTICLE, VTU, K3, INSTRUCTION, ISO2, MRTU, OST, RST, SPECIFICATION, STU, TR }.OrderByDescending(x => x.Length).ToList();

    public static readonly IReadOnlyList<string> DocumentStandards = new List<string> { STANDARD_AFNOR, STANDARD_ASTM, STANDARD_AS, STANDARD_A, STANDARD_BDS, STANDARD_BS, STANDARD_CSN, STANDARD_DIN, STANDARD_EN, STANDARD_GB, STANDARD_JIS, STANDARD_MSZ, STANDARD_ONORM, STANDARD_PNH, STANDARD_STAS, STANDARD_UNE, STANDARD_UNI, STANDARD }.OrderByDescending(x => x.Length).ToList();

    public static readonly IReadOnlyList<string> StandardsTitleCase = new List<string> { ARTICLE, INSTRUCTION, SPECIFICATION, STANDARD }.OrderByDescending(x => x.Length).ToList();

    public static IReadOnlyDictionary<string, string> DocumentTuStandards = new Dictionary<string, string>
    {
      { TU02,  CatalogConstants.GROUP_DOCUMENT_TU02 },
      { TU14,  CatalogConstants.GROUP_DOCUMENT_TU14 },
      { TU16,  CatalogConstants.GROUP_DOCUMENT_TU16 },
      { TU1,   CatalogConstants.GROUP_DOCUMENT_TU1 },
      { TU2,   CatalogConstants.GROUP_DOCUMENT_TU2 },
      { TU38,  CatalogConstants.GROUP_DOCUMENT_TU38 },
      { TU48,  CatalogConstants.GROUP_DOCUMENT_TU48 },
      { TU6,   CatalogConstants.GROUP_DOCUMENT_TU6 },
      { TU_RB, CatalogConstants.GROUP_DOCUMENT_TU_RB },
      { TU_BY, CatalogConstants.GROUP_DOCUMENT_TU_RB }
    };
  }
}