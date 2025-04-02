using System;
using System.Collections.Generic;

namespace LCAx
{

    public partial class Lcax
    {
        public Dictionary<string, AssemblySource> Assemblies { get; set; }
        public string ClassificationSystem { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public string FormatVersion { get; set; }
        public string Id { get; set; }
        public ImpactCategoryKey[] ImpactCategories { get; set; }
        public string LciaMethod { get; set; }
        public LifeCycleStage[] LifeCycleStages { get; set; }
        public Location Location { get; set; }
        public Dictionary<string, string> MetaData { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public ProjectInfo ProjectInfo { get; set; }
        public ProjectPhase ProjectPhase { get; set; }
        public long? ReferenceStudyPeriod { get; set; }
        public Dictionary<string, Dictionary<string, double?>> Results { get; set; }
        public SoftwareInfo SoftwareInfo { get; set; }

        public Lcax()
        {
            Assemblies = new Dictionary<string, AssemblySource>();
            ClassificationSystem = string.Empty;
            Comment = string.Empty;
            Description = string.Empty;
            FormatVersion = string.Empty;
            Id = string.Empty;
            ImpactCategories = new ImpactCategoryKey[0];
            LciaMethod = string.Empty;
            LifeCycleStages = new LifeCycleStage[0];
            Location = new Location();
            MetaData = new Dictionary<string, string>();
            Name = string.Empty;
            Owner = string.Empty;
            ProjectInfo = new ProjectInfo();
            ProjectPhase = new ProjectPhase();
            ReferenceStudyPeriod = null;
            Results = new Dictionary<string, Dictionary<string, double?>>();
            SoftwareInfo = new SoftwareInfo();
        }

    }

    public partial class AssemblySource
    {
        public Assembly Assembly { get; set; }
        public Reference Reference { get; set; }
        public AssemblySource()
        {
            Assembly = new Assembly();
            Reference = new Reference();
        }
    }

    public partial class Assembly
    {
        public Classification[] Classification { get; set; }
        public string Comment { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public Dictionary<string, string> MetaData { get; set; }
        public string Name { get; set; }
        public Dictionary<string, ProductSource> Products { get; set; }
        public double Quantity { get; set; }
        public Dictionary<string, Dictionary<string, double?>> Results { get; set; }
        public Unit Unit { get; set; }

        public Assembly()
        {
            Classification = new Classification[0];
            Comment = string.Empty;
            Description = string.Empty;
            Id = string.Empty;
            MetaData = new Dictionary<string, string>();
            Name = string.Empty;
            Products = new Dictionary<string, ProductSource>();
            Quantity = 0.0;
            Results = new Dictionary<string, Dictionary<string, double?>>();
            Unit = new Unit();
        }
    }

    public partial class Classification
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string System { get; set; }

        public Classification()
        {
            Code = string.Empty;
            Name = string.Empty;
            System = string.Empty;
        }
    }

    public partial class ProductSource
    {
        public Product Product { get; set; }
        public Reference Reference { get; set; }

        public ProductSource()
        {
            Product = new Product();
            Reference = new Reference();
        }
    }

    public partial class Product
    {
        public string Description { get; set; }
        public string Id { get; set; }
        public ImpactDataSource ImpactData { get; set; }
        public Dictionary<string, string> MetaData { get; set; }
        public string Name { get; set; }
        public double Quantity { get; set; }
        public long ReferenceServiceLife { get; set; }
        public Dictionary<string, Dictionary<string, double?>> Results { get; set; }
        public Transport[] Transport { get; set; }
        public Unit Unit { get; set; }

        public Product()
        {
            Description = string.Empty;
            Id = string.Empty;
            ImpactData = new ImpactDataSource();
            MetaData = new Dictionary<string, string>();
            Name = string.Empty;
            Quantity = 0.0;
            ReferenceServiceLife = 0;
            Results = new Dictionary<string, Dictionary<string, double?>>();
            Transport = new Transport[0];
            Unit = new Unit();
        }
    }

    public partial class ImpactDataSource
    {
        public Epd Epd { get; set; }
        public TechFlow TechFlow { get; set; }
        public Reference Reference { get; set; }

        public ImpactDataSource()
        {
            Epd = new Epd();
            TechFlow = new TechFlow();
            Reference = new Reference();
        }
    }

    public partial class Epd
    {
        public string Comment { get; set; }
        public Conversion[] Conversions { get; set; }
        public Unit DeclaredUnit { get; set; }
        public string FormatVersion { get; set; }
        public string Id { get; set; }
        public Dictionary<string, Dictionary<string, double?>> Impacts { get; set; }
        public Country Location { get; set; }
        public Dictionary<string, string> MetaData { get; set; }
        public string Name { get; set; }
        public DateTimeOffset PublishedDate { get; set; }
        public long? ReferenceServiceLife { get; set; }
        public Source Source { get; set; }
        public Standard Standard { get; set; }
        public SubType Subtype { get; set; }
        public DateTimeOffset ValidUntil { get; set; }
        public string Version { get; set; }

        public Epd()
        {
            Comment = string.Empty;
            Conversions = new Conversion[0];
            DeclaredUnit = new Unit();
            FormatVersion = string.Empty;
            Id = string.Empty;
            Impacts = new Dictionary<string, Dictionary<string, double?>>();
            Location = new Country();
            MetaData = new Dictionary<string, string>();
            Name = string.Empty;
            PublishedDate = DateTimeOffset.MinValue;
            ReferenceServiceLife = null;
            Source = new Source();
            Standard = new Standard();
            Subtype = new SubType();
            ValidUntil = DateTimeOffset.MinValue;
            Version = string.Empty;
        }
    }

    public partial class Conversion
    {
        public string MetaData { get; set; }
        public Unit To { get; set; }
        public double Value { get; set; }

        public Conversion()
        {
            MetaData = string.Empty;
            To = new Unit();
            Value = 0.0;
        }
    }

    public partial class Source
    {
        public string Name { get; set; }
        public string Url { get; set; }

        public Source()
        {
            Name = string.Empty;
            Url = string.Empty;
        }
    }

    public partial class Reference
    {
        public string Format { get; set; }
        public Dictionary<string, string> Overrides { get; set; }
        public string Path { get; set; }
        public ReferenceType Type { get; set; }
        public string Version { get; set; }

        public Reference()
        {
            Format = string.Empty;
            Overrides = new Dictionary<string, string>();
            Path = string.Empty;
            Type = new ReferenceType();
            Version = string.Empty;
        }
    }

    public partial class TechFlow
    {
        public string Comment { get; set; }
        public Conversion[] Conversions { get; set; }
        public Unit DeclaredUnit { get; set; }
        public string FormatVersion { get; set; }
        public string Id { get; set; }
        public Dictionary<string, Dictionary<string, double?>> Impacts { get; set; }
        public Country Location { get; set; }
        public Dictionary<string, string> MetaData { get; set; }
        public string Name { get; set; }
        public Source Source { get; set; }

        public TechFlow()
        {
            Comment = string.Empty;
            Conversions = new Conversion[0];
            DeclaredUnit = new Unit();
            FormatVersion = string.Empty;
            Id = string.Empty;
            Impacts = new Dictionary<string, Dictionary<string, double?>>();
            Location = new Country();
            MetaData = new Dictionary<string, string>();
            Name = string.Empty;
            Source = new Source();
        }
    }

    public partial class Transport
    {
        public double Distance { get; set; }
        public Unit DistanceUnit { get; set; }
        public string Id { get; set; }
        public ImpactDataSource ImpactData { get; set; }
        public LifeCycleStage[] LifeCycleStages { get; set; }
        public string Name { get; set; }

        public Transport()
        {
            Distance = 0.0;
            DistanceUnit = new Unit();
            Id = string.Empty;
            ImpactData = new ImpactDataSource();
            LifeCycleStages = new LifeCycleStage[0];
            Name = string.Empty;
        }
    }

    public partial class Location
    {
        public string Address { get; set; }
        public string City { get; set; }
        public Country Country { get; set; }

        public Location()
        {
            Address = string.Empty;
            City = string.Empty;
            Country = new Country();
        }
    }

    public partial class ProjectInfo
    {
        public BuildingInfo BuildingInfo { get; set; }
        public Dictionary<string, string> InfrastructureInfo { get; set; }

        public ProjectInfo()
        {
            BuildingInfo = new BuildingInfo();
            InfrastructureInfo = new Dictionary<string, string>();
        }

    }

    public partial class BuildingInfo
    {
        public long? BuildingCompletionYear { get; set; }
        public ValueUnit BuildingFootprint { get; set; }
        public ValueUnit BuildingHeight { get; set; }
        public ValueUnit BuildingMass { get; set; }
        public BuildingModelScope BuildingModelScope { get; set; }
        public long? BuildingPermitYear { get; set; }
        public BuildingType BuildingType { get; set; }
        public BuildingTypology BuildingTypology { get; set; }
        public long? BuildingUsers { get; set; }
        public string[] Certifications { get; set; }
        public double? EnergyDemandElectricity { get; set; }
        public double? EnergyDemandHeating { get; set; }
        public double? EnergySupplyElectricity { get; set; }
        public double? EnergySupplyHeating { get; set; }
        public double? ExportedElectricity { get; set; }
        public long FloorsAboveGround { get; set; }
        public long? FloorsBelowGround { get; set; }
        public string FrameType { get; set; }
        public GeneralEnergyClass GeneralEnergyClass { get; set; }
        public AreaType GrossFloorArea { get; set; }
        public AreaType HeatedFloorArea { get; set; }
        public string LocalEnergyClass { get; set; }
        public RoofType RoofType { get; set; }

        public BuildingInfo()
        {
            BuildingCompletionYear = null;
            BuildingFootprint = new ValueUnit();
            BuildingHeight = new ValueUnit();
            BuildingMass = new ValueUnit();
            BuildingModelScope = new BuildingModelScope();
            BuildingPermitYear = null;
            BuildingType = new BuildingType();
            BuildingTypology = new BuildingTypology();
            BuildingUsers = null;
            Certifications = new string[0];
            EnergyDemandElectricity = null;
            EnergyDemandHeating = null;
            EnergySupplyElectricity = null;
            EnergySupplyHeating = null;
            ExportedElectricity = null;
            FloorsAboveGround = 0;
            FloorsBelowGround = null;
            FrameType = string.Empty;
            GeneralEnergyClass = new GeneralEnergyClass();
            GrossFloorArea = new AreaType();
            HeatedFloorArea = new AreaType();
            LocalEnergyClass = string.Empty;
            RoofType = new RoofType();
        }
    }

    public partial class ValueUnit
    {
        public Unit Unit { get; set; }
        public double Value { get; set; }

        public ValueUnit()
        {
            Unit = new Unit();
            Value = 0.0;
        }
    }

    public partial class BuildingModelScope
    {
        public bool BuildingServices { get; set; }
        public bool ExternalWorks { get; set; }
        public bool FacilitatingWorks { get; set; }
        public bool FfE { get; set; }
        public bool Finishes { get; set; }
        public bool Substructure { get; set; }
        public bool SuperstructureEnvelope { get; set; }
        public bool SuperstructureFrame { get; set; }
        public bool SuperstructureInternalElements { get; set; }


        public BuildingModelScope()
        {
            BuildingServices = false;
            ExternalWorks = false;
            FacilitatingWorks = false;
            FfE = false;
            Finishes = false;
            Substructure = false;
            SuperstructureEnvelope = false;
            SuperstructureFrame = false;
            SuperstructureInternalElements = false;
        }
    }

    public partial class AreaType
    {
        public string Definition { get; set; }
        public Unit Unit { get; set; }
        public double Value { get; set; }

        public AreaType()
        {
            Definition = string.Empty;
            Unit = new Unit();
            Value = 0.0;
        }
    }

    public partial class SoftwareInfo
    {
        public string CalculationType { get; set; }
        public string GoalAndScopeDefinition { get; set; }
        public string LcaSoftware { get; set; }
        public SoftwareInfo()
        {
            CalculationType = string.Empty;
            GoalAndScopeDefinition = string.Empty;
            LcaSoftware = string.Empty;
        }
    }



    public enum Unit { Kg, Km, Kwh, L, M, M2, M2R1, M3, Pcs, Tones, TonesKm, Unknown };

    public enum Country { Abw, Afg, Ago, Aia, Ala, Alb, And, Are, Arg, Arm, Asm, Ata, Atf, Atg, Aus, Aut, Aze, Bdi, Bel, Ben, Bes, Bfa, Bgd, Bgr, Bhr, Bhs, Bih, Blm, Blr, Blz, Bmu, Bol, Bra, Brb, Brn, Btn, Bvt, Bwa, Caf, Can, Cck, Che, Chl, Chn, Civ, Cmr, Cod, Cog, Cok, Col, Com, Cpv, Cri, Cub, Cuw, Cxr, Cym, Cyp, Cze, Deu, Dji, Dma, Dnk, Dom, Dza, Ecu, Egy, Eri, Esh, Esp, Est, Eth, Fin, Fji, Flk, Fra, Fro, Fsm, Gab, Gbr, Geo, Ggy, Gha, Gib, Gin, Glp, Gmb, Gnb, Gnq, Grc, Grd, Grl, Gtm, Guf, Gum, Guy, Hkg, Hmd, Hnd, Hrv, Hti, Hun, Idn, Imn, Ind, Iot, Irl, Irn, Irq, Isl, Isr, Ita, Jam, Jey, Jor, Jpn, Kaz, Ken, Kgz, Khm, Kir, Kna, Kor, Kwt, Lao, Lbn, Lbr, Lby, Lca, Lie, Lka, Lso, Ltu, Lux, Lva, Mac, Maf, Mar, Mco, Mda, Mdg, Mdv, Mex, Mhl, Mkd, Mli, Mlt, Mmr, Mne, Mng, Mnp, Moz, Mrt, Msr, Mtq, Mus, Mwi, Mys, Myt, Nam, Ncl, Ner, Nfk, Nga, Nic, Niu, Nld, Nor, Npl, Nru, Nzl, Omn, Pak, Pan, Pcn, Per, Phl, Plw, Png, Pol, Pri, Prk, Prt, Pry, Pse, Pyf, Qat, Reu, Rou, Rus, Rwa, Sau, Sdn, Sen, Sgp, Sgs, Shn, Sjm, Slb, Sle, Slv, Smr, Som, Spm, Srb, Ssd, Stp, Sur, Svk, Svn, Swe, Swz, Sxm, Syc, Syr, Tca, Tcd, Tgo, Tha, Tjk, Tkl, Tkm, Tls, Ton, Tto, Tun, Tur, Tuv, Twn, Tza, Uga, Ukr, Umi, Unknown, Ury, Usa, Uzb, Vat, Vct, Ven, Vgb, Vir, Vnm, Vut, Wlf, Wsm, Yem, Zaf, Zmb, Zwe };

    public enum Standard { En15804A1, En15804A2, Unknown };

    public enum SubType { Generic, Industry, Representative, Specific };

    public enum ReferenceType { External, Internal };

    public enum LifeCycleStage { A1A3, A4, A5, B1, B2, B3, B4, B5, B6, B7, C1, C2, C3, C4, D };

    public enum ImpactCategoryKey { Adpe, Adpf, Ap, Cru, Eee, Eet, Ep, EpFw, EpMar, EpTer, EtpFw, Fw, Gwp, GwpBio, GwpFos, GwpLul, HtpC, HtpNc, Hwd, Irp, Mer, Mrf, Nhwd, Nrsf, Odp, Penre, Penrm, Penrt, Pere, Perm, Pert, Pm, Pocp, Rsf, Rwd, Sm, Sqp, Wdp };

    public enum BuildingType { New, Renovation };

    public enum BuildingTypology { Agricultural, Commercial, Industrial, Infrastructure, Mixeduse, Office, Other, Public, Residential };

    public enum GeneralEnergyClass { Advanced, Existing, Standard, Unknown };

    public enum RoofType { Flat, Other, Pitched, Pyramid, Saddle };

    public enum ProjectPhase { Built, Design, Ongoing, Other };
}