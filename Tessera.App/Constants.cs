using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tessera.App
{
  internal class Constants
  {
    public const string POLYNOM_CLIENT_ID = "A6E64FA3-51DB-42D3-9AD7-B82E828EEC31";
    public const string REFENCE_NAME = "Материалы и Сортаменты";
    public const string CATALOG_MATERIAL = "Материалы";
    public const string CATALOG_SORTAMENT = "Сортаменты";
    public const string CATALOG_SORTAMENT_EX = "Экземпляры сортаментов";
    public const string CATALOG_TYPE_SIZE = "Типоразмеры";

    public const string PROP_MATERIAL_MASK = "c:@Materials::c:Materials::pd:MaterialsMark";
    public const string PROP_SORTAMENT_MASK = "c:Sortaments::pd:MaterialsMark";
    public const string PROP_SHAPE_MIS = "c:ShapesMiS::pd:ShapeMiS";
    public const string PROP_NAME_AND_DESCRIPTION = "c:@NameAndDescription::pd:@Name";
    public const string PROP_NAME_AND_DESCRIPTION_ABSOLUTE_CODE = "c:@NameAndDescription::c:@ClassificationItem::pd:@Name";

    public const string CONCEPT_SORTAMENT = "c:Sortaments";
    public const string CONCEPT_MATERIAL = "c:@Materials::c:Materials";
    public const string CONCEPT_SHAPE = "c:ShapesMiS";
    public const string CONCEPT_CLASSIFICATION_ITEM = "c:@NameAndDescription::c:@ClassificationItem";

    public const string LINK_SORTAMENT_MATERIAL = "ld:ExMatLinkCode";
    public const string LINK_TYPESIZE_SORTAMENT = "ld:SizeSortLinkCode";
    public const string LINK_SORTAMENTEX_MATERIAL = "ld:ExSortMatLinkCode";
    public const string LINK_SORTAMENTEX_SORTAMENT = "ld:ExSortSortLinkCode";
    public const string LINK_SORTAMENTEX_TYPE_SIZE = "ld:ExSortSizeLinkCode";

    public const string GOST = "ГОСТ";
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
    public const string ASTM = "ASTM";

    public static readonly List<string> Standards = new() { GOST, GOST_R, TU, OST, STO, RD, SP, SNiP, TR_TS, TR_EAS, PNST, SANPIN, GN, MU, FS, ND, MK, MK2, ISO, EN, ASTM };
  }
}
