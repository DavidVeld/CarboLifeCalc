using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

//
// LCAx data model, shaped to LCAx_json_Scheme.txt (draft-07) sitting beside this file.
//
// Three things about that schema drive the whole layout here and are worth stating once:
//
// 1. Every name is lowerCamelCase, so every property carries an explicit JsonPropertyName.
//    Relying on a naming policy instead would leave the dictionary keys, which are data,
//    exposed to the same transformation.
//
// 2. assemblies, products and impactData are "oneOf" unions of an inline object carrying
//    "type": "actual" and a {"type": "reference", "uri": ...} pointer. The fields of the
//    actual variant sit directly on the object, they are NOT nested under an "assembly" or
//    "product" key. That is why the AssemblySource / ProductSource / ImpactDataSource
//    wrappers this file used to carry are gone: they serialised one level too deep and no
//    validator would have accepted the result.
//
// 3. Every enum is a string, and the strings are not the C# member names. LcaxEnumConverter
//    below lowercases the member name, which is already correct for the great majority of
//    them (Country in particular, where Gbr -> "gbr" all the way down), and anything that
//    needs a different spelling carries a JsonEnumValue attribute.
//
namespace LCAx
{
    /// <summary>
    /// Overrides the JSON spelling of a single enum member. Without one, a member serialises
    /// as its own name lowercased.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class JsonEnumValueAttribute : Attribute
    {
        public string Value { get; private set; }

        public JsonEnumValueAttribute(string value)
        {
            Value = value;
        }
    }

    internal sealed class LcaxEnumConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsEnum;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type converterType = typeof(LcaxEnumConverter<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType);
        }
    }

    internal sealed class LcaxEnumConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        private static readonly Dictionary<T, string> toJson;
        private static readonly Dictionary<string, T> fromJson;

        static LcaxEnumConverter()
        {
            toJson = new Dictionary<T, string>();
            fromJson = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

            foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                T value = (T)field.GetValue(null);

                JsonEnumValueAttribute attribute =
                    (JsonEnumValueAttribute)Attribute.GetCustomAttribute(field, typeof(JsonEnumValueAttribute));

                string text = attribute != null
                    ? attribute.Value
                    : field.Name.ToLowerInvariant();

                if (toJson.ContainsKey(value) == false)
                    toJson.Add(value, text);

                if (fromJson.ContainsKey(text) == false)
                    fromJson.Add(text, value);
            }
        }

        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string text = reader.GetString();

            T value;
            if (text != null && fromJson.TryGetValue(text, out value))
                return value;

            return default(T);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(ToJsonValue(value));
        }

        internal static string ToJsonValue(T value)
        {
            string text;
            if (toJson.TryGetValue(value, out text))
                return text;

            return value.ToString().ToLowerInvariant();
        }
    }

    /// <summary>
    /// The schema spelling of an enum member, for use as a dictionary key. impacts and results
    /// are keyed by impact category and life cycle stage, and those keys have to carry exactly
    /// the same strings the enums serialise to, so build them from here rather than by hand.
    /// </summary>
    public static class LcaxKey
    {
        public static string Of<T>(T value) where T : struct, Enum
        {
            return LcaxEnumConverter<T>.ToJsonValue(value);
        }
    }

    /// <summary>
    /// The schema asks for "format": "date" on publishedDate and validUntil, which is a plain
    /// yyyy-MM-dd, not the full offset timestamp a DateTime serialises to by default.
    /// </summary>
    internal sealed class LcaxDateConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string text = reader.GetString();

            DateTime value;
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out value))
                return value;

            return DateTime.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// The root LCAx object. Required by the schema: assemblies, formatVersion, id,
    /// impactCategories, lifeCycleStages, location, name, projectPhase, softwareInfo.
    /// </summary>
    public partial class Lcax
    {
        [JsonPropertyName("assemblies")]
        public Dictionary<string, Assembly> Assemblies { get; set; }

        [JsonPropertyName("classificationSystem")]
        public string ClassificationSystem { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("formatVersion")]
        public string FormatVersion { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("impactCategories")]
        public List<ImpactCategoryKey> ImpactCategories { get; set; }

        [JsonPropertyName("lciaMethod")]
        public string LciaMethod { get; set; }

        [JsonPropertyName("lifeCycleStages")]
        public List<LifeCycleStage> LifeCycleStages { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }

        [JsonPropertyName("metaData")]
        public Dictionary<string, string> MetaData { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("owner")]
        public string Owner { get; set; }

        [JsonPropertyName("projectInfo")]
        public ProjectInfo ProjectInfo { get; set; }

        [JsonPropertyName("projectPhase")]
        public ProjectPhase ProjectPhase { get; set; }

        /// <summary>
        /// uint8 in the schema, so 0 to 255 years.
        /// </summary>
        [JsonPropertyName("referenceStudyPeriod")]
        public int? ReferenceStudyPeriod { get; set; }

        [JsonPropertyName("results")]
        public Dictionary<string, Dictionary<string, double?>> Results { get; set; }

        [JsonPropertyName("softwareInfo")]
        public SoftwareInfo SoftwareInfo { get; set; }

        public Lcax()
        {
            Assemblies = new Dictionary<string, Assembly>();
            FormatVersion = string.Empty;
            Id = string.Empty;
            ImpactCategories = new List<ImpactCategoryKey>();
            LifeCycleStages = new List<LifeCycleStage>();
            Location = new Location();
            Name = string.Empty;
            ProjectPhase = ProjectPhase.Other;
            SoftwareInfo = new SoftwareInfo();
        }
    }

    /// <summary>
    /// The "actual" branch of ReferenceSource_for_Assembly. Required: id, name, products,
    /// quantity, type, unit.
    /// </summary>
    public partial class Assembly
    {
        [JsonPropertyName("type")]
        public string Type { get { return "actual"; } }

        [JsonPropertyName("classification")]
        public List<Classification> Classification { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("metaData")]
        public Dictionary<string, string> MetaData { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("products")]
        public Dictionary<string, Product> Products { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        [JsonPropertyName("results")]
        public Dictionary<string, Dictionary<string, double?>> Results { get; set; }

        [JsonPropertyName("unit")]
        public Unit Unit { get; set; }

        public Assembly()
        {
            Id = string.Empty;
            Name = string.Empty;
            Products = new Dictionary<string, Product>();
            Quantity = 0.0;
            Unit = Unit.Unknown;
        }
    }

    /// <summary>
    /// The "actual" branch of ReferenceSource_for_Product. Required: id, impactData, name,
    /// quantity, referenceServiceLife, type, unit.
    /// </summary>
    public partial class Product
    {
        [JsonPropertyName("type")]
        public string Type { get { return "actual"; } }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("impactData")]
        public Epd ImpactData { get; set; }

        [JsonPropertyName("metaData")]
        public Dictionary<string, string> MetaData { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; }

        /// <summary>
        /// uint32 in the schema, so it may not go negative.
        /// </summary>
        [JsonPropertyName("referenceServiceLife")]
        public long ReferenceServiceLife { get; set; }

        [JsonPropertyName("results")]
        public Dictionary<string, Dictionary<string, double?>> Results { get; set; }

        [JsonPropertyName("transport")]
        public List<Transport> Transport { get; set; }

        [JsonPropertyName("unit")]
        public Unit Unit { get; set; }

        public Product()
        {
            Id = string.Empty;
            ImpactData = new Epd();
            Name = string.Empty;
            Quantity = 0.0;
            ReferenceServiceLife = 0;
            Unit = Unit.Unknown;
        }
    }

    /// <summary>
    /// An EPD carrying the "actual" discriminator, which is what satisfies the
    /// ReferenceSource_for_ImpactDataSource union. Required: declaredUnit, formatVersion, id,
    /// impacts, location, name, publishedDate, standard, subtype, validUntil, version.
    /// </summary>
    public partial class Epd
    {
        [JsonPropertyName("type")]
        public string Type { get { return "actual"; } }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("conversions")]
        public List<Conversion> Conversions { get; set; }

        [JsonPropertyName("declaredUnit")]
        public Unit DeclaredUnit { get; set; }

        [JsonPropertyName("formatVersion")]
        public string FormatVersion { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("impacts")]
        public Dictionary<string, Dictionary<string, double?>> Impacts { get; set; }

        [JsonPropertyName("location")]
        public Country Location { get; set; }

        [JsonPropertyName("metaData")]
        public Dictionary<string, string> MetaData { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("publishedDate")]
        [JsonConverter(typeof(LcaxDateConverter))]
        public DateTime PublishedDate { get; set; }

        [JsonPropertyName("referenceServiceLife")]
        public long? ReferenceServiceLife { get; set; }

        [JsonPropertyName("source")]
        public Source Source { get; set; }

        [JsonPropertyName("standard")]
        public Standard Standard { get; set; }

        [JsonPropertyName("subtype")]
        public SubType Subtype { get; set; }

        [JsonPropertyName("validUntil")]
        [JsonConverter(typeof(LcaxDateConverter))]
        public DateTime ValidUntil { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        public Epd()
        {
            DeclaredUnit = Unit.Unknown;
            FormatVersion = string.Empty;
            Id = string.Empty;
            Impacts = new Dictionary<string, Dictionary<string, double?>>();
            Location = Country.Unknown;
            Name = string.Empty;
            PublishedDate = DateTime.Today;
            Standard = Standard.Unknown;
            Subtype = SubType.Generic;
            ValidUntil = DateTime.Today;
            Version = string.Empty;
        }
    }

    public partial class Classification
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("system")]
        public string System { get; set; }

        public Classification()
        {
            Code = string.Empty;
            Name = string.Empty;
            System = string.Empty;
        }
    }

    public partial class Conversion
    {
        [JsonPropertyName("metaData")]
        public string MetaData { get; set; }

        [JsonPropertyName("to")]
        public Unit To { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }

        public Conversion()
        {
            MetaData = string.Empty;
            To = Unit.Unknown;
            Value = 0.0;
        }
    }

    public partial class Source
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        public Source()
        {
            Name = string.Empty;
        }
    }

    public partial class Transport
    {
        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("distanceUnit")]
        public Unit DistanceUnit { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("impactData")]
        public Epd ImpactData { get; set; }

        [JsonPropertyName("lifeCycleStages")]
        public List<LifeCycleStage> LifeCycleStages { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        public Transport()
        {
            Distance = 0.0;
            DistanceUnit = Unit.Km;
            Id = string.Empty;
            ImpactData = new Epd();
            LifeCycleStages = new List<LifeCycleStage>();
            Name = string.Empty;
        }
    }

    public partial class Location
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("country")]
        public Country Country { get; set; }

        public Location()
        {
            Country = Country.Unknown;
        }
    }

    /// <summary>
    /// The "buildingInfo" branch of the ProjectInfo union: the building fields sit inline
    /// alongside the discriminator rather than under a nested buildingInfo object. Required:
    /// buildingType, buildingTypology, floorsAboveGround, generalEnergyClass, roofType, type.
    /// </summary>
    public partial class ProjectInfo
    {
        [JsonPropertyName("type")]
        public string Type { get { return "buildingInfo"; } }

        [JsonPropertyName("buildingCompletionYear")]
        public long? BuildingCompletionYear { get; set; }

        [JsonPropertyName("buildingFootprint")]
        public ValueUnit BuildingFootprint { get; set; }

        [JsonPropertyName("buildingHeight")]
        public ValueUnit BuildingHeight { get; set; }

        [JsonPropertyName("buildingMass")]
        public ValueUnit BuildingMass { get; set; }

        [JsonPropertyName("buildingModelScope")]
        public List<BuildingModelScope> BuildingModelScope { get; set; }

        [JsonPropertyName("buildingPermitYear")]
        public long? BuildingPermitYear { get; set; }

        [JsonPropertyName("buildingType")]
        public BuildingType BuildingType { get; set; }

        [JsonPropertyName("buildingTypology")]
        public List<BuildingTypology> BuildingTypology { get; set; }

        [JsonPropertyName("buildingUsers")]
        public long? BuildingUsers { get; set; }

        [JsonPropertyName("certifications")]
        public List<string> Certifications { get; set; }

        [JsonPropertyName("energyDemandElectricity")]
        public double? EnergyDemandElectricity { get; set; }

        [JsonPropertyName("energyDemandHeating")]
        public double? EnergyDemandHeating { get; set; }

        [JsonPropertyName("energySupplyElectricity")]
        public double? EnergySupplyElectricity { get; set; }

        [JsonPropertyName("energySupplyHeating")]
        public double? EnergySupplyHeating { get; set; }

        [JsonPropertyName("exportedElectricity")]
        public double? ExportedElectricity { get; set; }

        [JsonPropertyName("floorsAboveGround")]
        public int FloorsAboveGround { get; set; }

        [JsonPropertyName("floorsBelowGround")]
        public int? FloorsBelowGround { get; set; }

        [JsonPropertyName("frameType")]
        public string FrameType { get; set; }

        [JsonPropertyName("generalEnergyClass")]
        public GeneralEnergyClass GeneralEnergyClass { get; set; }

        [JsonPropertyName("grossFloorArea")]
        public AreaType GrossFloorArea { get; set; }

        [JsonPropertyName("heatedFloorArea")]
        public AreaType HeatedFloorArea { get; set; }

        [JsonPropertyName("localEnergyClass")]
        public string LocalEnergyClass { get; set; }

        [JsonPropertyName("roofType")]
        public RoofType RoofType { get; set; }

        public ProjectInfo()
        {
            BuildingType = LCAx.BuildingType.Other;
            BuildingTypology = new List<BuildingTypology>();
            FloorsAboveGround = 0;
            GeneralEnergyClass = GeneralEnergyClass.Unknown;
            RoofType = RoofType.Other;
        }
    }

    public partial class ValueUnit
    {
        [JsonPropertyName("unit")]
        public Unit Unit { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }

        public ValueUnit()
        {
            Unit = Unit.Unknown;
            Value = 0.0;
        }
    }

    public partial class AreaType
    {
        [JsonPropertyName("definition")]
        public string Definition { get; set; }

        [JsonPropertyName("unit")]
        public Unit Unit { get; set; }

        [JsonPropertyName("value")]
        public double Value { get; set; }

        public AreaType()
        {
            Definition = string.Empty;
            Unit = Unit.M2;
            Value = 0.0;
        }
    }

    public partial class SoftwareInfo
    {
        [JsonPropertyName("calculationType")]
        public string CalculationType { get; set; }

        [JsonPropertyName("goalAndScopeDefinition")]
        public string GoalAndScopeDefinition { get; set; }

        [JsonPropertyName("lcaSoftware")]
        public string LcaSoftware { get; set; }

        public SoftwareInfo()
        {
            LcaSoftware = string.Empty;
        }
    }

    //
    // Enums. A member with no JsonEnumValue serialises as its own name lowercased, which the
    // schema spelling already matches, Country included.
    //

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum Unit
    {
        Unknown, M, M2, M3, Kg, Tones, Pcs, Kwh, L, M2R1, Km,
        [JsonEnumValue("tones_km")] TonesKm,
        Kgm3
    };

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum Country
    {
        Unknown, Abw, Afg, Ago, Aia, Ala, Alb, And, Are, Arg, Arm, Asm, Ata, Atf, Atg, Aus, Aut, Aze, Bdi, Bel, Ben,
        Bes, Bfa, Bgd, Bgr, Bhr, Bhs, Bih, Blm, Blr, Blz, Bmu, Bol, Bra, Brb, Brn, Btn, Bvt, Bwa, Caf, Can, Cck, Che,
        Chl, Chn, Civ, Cmr, Cod, Cog, Cok, Col, Com, Cpv, Cri, Cub, Cuw, Cxr, Cym, Cyp, Cze, Deu, Dji, Dma, Dnk, Dom,
        Dza, Ecu, Egy, Eri, Esh, Esp, Est, Eth, Fin, Fji, Flk, Fra, Fro, Fsm, Gab, Gbr, Geo, Ggy, Gha, Gib, Gin, Glp,
        Gmb, Gnb, Gnq, Grc, Grd, Grl, Gtm, Guf, Gum, Guy, Hkg, Hmd, Hnd, Hrv, Hti, Hun, Idn, Imn, Ind, Iot, Irl, Irn,
        Irq, Isl, Isr, Ita, Jam, Jey, Jor, Jpn, Kaz, Ken, Kgz, Khm, Kir, Kna, Kor, Kwt, Lao, Lbn, Lbr, Lby, Lca, Lie,
        Lka, Lso, Ltu, Lux, Lva, Mac, Maf, Mar, Mco, Mda, Mdg, Mdv, Mex, Mhl, Mkd, Mli, Mlt, Mmr, Mne, Mng, Mnp, Moz,
        Mrt, Msr, Mtq, Mus, Mwi, Mys, Myt, Nam, Ncl, Ner, Nfk, Nga, Nic, Niu, Nld, Nor, Npl, Nru, Nzl, Omn, Pak, Pan,
        Pcn, Per, Phl, Plw, Png, Pol, Pri, Prk, Prt, Pry, Pse, Pyf, Qat, Reu, Rou, Rus, Rwa, Sau, Sdn, Sen, Sgp, Sgs,
        Shn, Sjm, Slb, Sle, Slv, Smr, Som, Spm, Srb, Ssd, Stp, Sur, Svk, Svn, Swe, Swz, Sxm, Syc, Syr, Tca, Tcd, Tgo,
        Tha, Tjk, Tkl, Tkm, Tls, Ton, Tto, Tun, Tur, Tuv, Twn, Tza, Uga, Ukr, Umi, Ury, Usa, Uzb, Vat, Vct, Ven, Vgb,
        Vir, Vnm, Vut, Wlf, Wsm, Yem, Zaf, Zmb, Zwe
    };

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum Standard { Unknown, En15804A1, En15804A2 };

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum SubType { Generic, Industry, Representative, Specific };

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum LifeCycleStage { A0, A1A3, A4, A5, B1, B2, B3, B4, B5, B6, B7, B8, C1, C2, C3, C4, D };

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum ImpactCategoryKey
    {
        Gwp,
        [JsonEnumValue("gwp_fos")] GwpFos,
        [JsonEnumValue("gwp_bio")] GwpBio,
        [JsonEnumValue("gwp_lul")] GwpLul,
        Odp, Ap, Ep,
        [JsonEnumValue("ep_fw")] EpFw,
        [JsonEnumValue("ep_mar")] EpMar,
        [JsonEnumValue("ep_ter")] EpTer,
        Pocp, Adpe, Adpf, Penre, Pere, Perm, Pert, Penrt, Penrm, Sm, Pm, Wdp, Irp,
        [JsonEnumValue("etp_fw")] EtpFw,
        [JsonEnumValue("htp_c")] HtpC,
        [JsonEnumValue("htp_nc")] HtpNc,
        Sqp, Rsf, Nrsf, Fw, Hwd, Nhwd, Rwd, Cru, Mrf, Mer, Eee, Eet
    };

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum BuildingType
    {
        [JsonEnumValue("new_construction_works")] NewConstructionWorks,
        [JsonEnumValue("demolition")] Demolition,
        [JsonEnumValue("deconstruction_and_new_construction_works")] DeconstructionAndNewConstructionWorks,
        [JsonEnumValue("retrofit_works")] RetrofitWorks,
        [JsonEnumValue("extension_works")] ExtensionWorks,
        [JsonEnumValue("retrofit_and_extension_works")] RetrofitAndExtensionWorks,
        [JsonEnumValue("fit_out_works")] FitOutWorks,
        [JsonEnumValue("operations")] Operations,
        [JsonEnumValue("other")] Other
    };

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum BuildingTypology { Office, Residential, Public, Commercial, Industrial, Infrastructure, Agricultural, Other };

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum BuildingModelScope
    {
        [JsonEnumValue("facilitating_works")] FacilitatingWorks,
        [JsonEnumValue("substructure")] Substructure,
        [JsonEnumValue("superstructure_frame")] SuperstructureFrame,
        [JsonEnumValue("superstructure_envelope")] SuperstructureEnvelope,
        [JsonEnumValue("superstructure_internal_elements")] SuperstructureInternalElements,
        [JsonEnumValue("finishes")] Finishes,
        [JsonEnumValue("building_services")] BuildingServices,
        [JsonEnumValue("external_works")] ExternalWorks,
        [JsonEnumValue("ff_e")] FfE
    };

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum GeneralEnergyClass { Unknown, Existing, Standard, Advanced };

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum RoofType { Flat, Pitched, Saddle, Pyramid, Other };

    [JsonConverter(typeof(LcaxEnumConverterFactory))]
    public enum ProjectPhase
    {
        [JsonEnumValue("strategic_design")] StrategicDesign,
        [JsonEnumValue("concept_design")] ConceptDesign,
        [JsonEnumValue("technical_design")] TechnicalDesign,
        [JsonEnumValue("construction")] Construction,
        [JsonEnumValue("post_completion")] PostCompletion,
        [JsonEnumValue("in_use")] InUse,
        [JsonEnumValue("other")] Other
    };
}
