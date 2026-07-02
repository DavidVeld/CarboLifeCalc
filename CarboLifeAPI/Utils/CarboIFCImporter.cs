using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CarboLifeAPI.Data;

namespace CarboLifeAPI
{
    /// <summary>
    /// Dependency-free importer that reads a STEP Part 21 IFC file (.ifc) and turns the
    /// physical building elements into <see cref="CarboElement"/> instances.
    ///
    /// No external libraries are used: the file is tokenised by hand and the IFC relationship
    /// graph is walked manually to recover material, quantities (volume / area) and the level.
    ///
    /// Volumes are returned in m3 and areas in m2, converted from whatever unit the IFC declares
    /// in its IfcUnitAssignment.
    ///
    /// IMPORTANT: volume and area only exist in the file if the exporter wrote base quantities.
    /// In Revit's IFC export setup tick "Export base quantities" (Property Sets tab), otherwise
    /// every element comes back with Volume = 0 (and is flagged includeInCalc = false).
    /// </summary>
    public class CarboIFCImporter
    {
        // ---- public knobs -------------------------------------------------------------------

        /// <summary>IFC entity types that are treated as importable physical elements.
        /// Add or remove freely; everything else (spaces, openings, annotations, geometry) is ignored.</summary>
        public HashSet<string> ElementTypes { get; }

        /// <summary>Non-fatal notes raised during the import (missing quantities, odd units, etc.).</summary>
        public List<string> Warnings { get; }

        /// <summary>The schema string found in the file header, e.g. "IFC4" or "IFC2X3".</summary>
        public string Schema { get; private set; }

        public CarboIFCImporter()
        {
            Warnings = new List<string>();
            ElementTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // structural
                "IFCWALL", "IFCWALLSTANDARDCASE",
                "IFCSLAB", "IFCSLABSTANDARDCASE", "IFCSLABELEMENTEDCASE",
                "IFCBEAM", "IFCBEAMSTANDARDCASE",
                "IFCCOLUMN", "IFCCOLUMNSTANDARDCASE",
                "IFCMEMBER", "IFCMEMBERSTANDARDCASE",
                "IFCPLATE", "IFCPLATESTANDARDCASE",
                "IFCFOOTING", "IFCPILE",
                "IFCREINFORCINGBAR", "IFCREINFORCINGMESH", "IFCTENDON",
                "IFCBUILDINGELEMENTPART",
                // architectural / general
                "IFCCOVERING", "IFCROOF",
                "IFCSTAIR", "IFCSTAIRFLIGHT", "IFCRAMP", "IFCRAMPFLIGHT",
                "IFCRAILING", "IFCCURTAINWALL",
                "IFCWINDOW", "IFCDOOR",
                "IFCBUILDINGELEMENTPROXY", "IFCCHIMNEY"
            };
        }

        // ---- entry points -------------------------------------------------------------------

        public List<CarboElement> Import(string ifcFilePath)
        {
            if (string.IsNullOrWhiteSpace(ifcFilePath) || !File.Exists(ifcFilePath))
                throw new FileNotFoundException("IFC file not found.", ifcFilePath);

            string text = File.ReadAllText(ifcFilePath);
            return ImportFromText(text);
        }

        public List<CarboElement> ImportFromText(string text)
        {
            var result = new List<CarboElement>();
            if (string.IsNullOrEmpty(text)) return result;

            // 1. parse every instance into raw form (lazy attribute parsing happens on demand)
            var model = new IfcModel(text);
            Schema = model.Schema;

            // 2. work out unit conversion factors from IfcUnitAssignment
            var units = ParseUnits(model);
            if (Math.Abs(units.LengthToM - 1.0) > 1e-9)
                Warnings.Add("IFC length unit is not metres (factor " + units.LengthToM.ToString("G4", CultureInfo.InvariantCulture) + " m/unit). Quantities converted accordingly.");

            // 3. build reverse-lookup indexes from the IfcRel* objects
            var materialOf = new Dictionary<int, int>();   // elementId -> relating material id
            var storeyOf = new Dictionary<int, int>();     // elementId -> building storey id
            var propsOf = new Dictionary<int, List<int>>(); // elementId -> property definition ids

            foreach (var id in model.AllIds)
            {
                string t = model.TypeOf(id);
                if (t == null || !t.StartsWith("IFCREL", StringComparison.Ordinal)) continue;

                if (t == "IFCRELASSOCIATESMATERIAL")
                {
                    var a = model.Attrs(id);
                    int matRef = GetRef(a, 5);
                    if (matRef > 0)
                        foreach (var eid in GetRefs(a, 4)) materialOf[eid] = matRef;
                }
                else if (t == "IFCRELCONTAINEDINSPATIALSTRUCTURE")
                {
                    var a = model.Attrs(id);
                    int structRef = GetRef(a, 5);
                    if (structRef > 0 && model.TypeOf(structRef) == "IFCBUILDINGSTOREY")
                        foreach (var eid in GetRefs(a, 4)) storeyOf[eid] = structRef;
                }
                else if (t == "IFCRELDEFINESBYPROPERTIES")
                {
                    var a = model.Attrs(id);
                    int defRef = GetRef(a, 5);
                    if (defRef <= 0) continue;
                    foreach (var eid in GetRefs(a, 4))
                    {
                        List<int> list;
                        if (!propsOf.TryGetValue(eid, out list)) { list = new List<int>(); propsOf[eid] = list; }
                        list.Add(defRef);
                    }
                }
            }

            // 4. cache storey elevation / name so we only resolve each once
            var storeyCache = new Dictionary<int, Storey>();

            int noVolumeCount = 0;
            int noTagCount = 0;

            // 5. build a CarboElement for every element of an importable type
            foreach (var id in model.AllIds)
            {
                string t = model.TypeOf(id);
                if (t == null || !ElementTypes.Contains(t)) continue;

                var a = model.Attrs(id);

                var ce = new CarboElement();

                // --- identity -----------------------------------------------------------------
                string guid = AsString(GetAttr(a, 0));
                string tag = AsString(GetAttr(a, 7));     // IfcElement.Tag = Revit ElementId
                long revitId;
                if (!long.TryParse(tag, NumberStyles.Any, CultureInfo.InvariantCulture, out revitId))
                {
                    revitId = id;                          // fall back to STEP line id (unique in file)
                    noTagCount++;
                }
                ce.Id = revitId;
                ce.GUID = guid ?? "";

                // --- naming -------------------------------------------------------------------
                string name = AsString(GetAttr(a, 2));
                string objType = AsString(GetAttr(a, 4));
                ce.Name = !string.IsNullOrEmpty(name) ? name : (objType ?? "");
                ce.Category = FriendlyType(t);
                ce.SubCategory = objType ?? "";

                // --- material -----------------------------------------------------------------
                string layerBreakdown = null;
                int matRef;
                if (materialOf.TryGetValue(id, out matRef))
                {
                    var mat = ResolveMaterial(model, matRef, units, new HashSet<int>());
                    if (mat != null && !string.IsNullOrEmpty(mat.Name))
                    {
                        ce.MaterialName = mat.Name;
                        ce.MaterialCategoryName = mat.Category ?? "";
                        layerBreakdown = mat.Breakdown;
                    }
                }

                // --- quantities (volume / area) and grade from property sets ------------------
                double volume = 0, area = 0;
                string grade = null;
                List<int> defs;
                if (propsOf.TryGetValue(id, out defs))
                    ExtractFromPropertyDefs(model, defs, units, ref volume, ref area, ref grade);

                ce.Volume = volume;
                ce.Volume_Total = volume;     // EC is computed from Volume_Total downstream
                ce.Area = area;
                if (!string.IsNullOrEmpty(grade)) ce.Grade = grade;
                if (volume <= 0) noVolumeCount++;

                // --- level --------------------------------------------------------------------
                int storeyRef;
                if (storeyOf.TryGetValue(id, out storeyRef))
                {
                    Storey s;
                    if (!storeyCache.TryGetValue(storeyRef, out s))
                    {
                        s = ReadStorey(model, storeyRef, units);
                        storeyCache[storeyRef] = s;
                    }
                    ce.Level = s.Elevation;
                    ce.LevelName = s.Name ?? "";
                }

                // --- bookkeeping --------------------------------------------------------------
                var ad = new StringBuilder();
                ad.Append("IFC:").Append(t);
                if (!string.IsNullOrEmpty(layerBreakdown)) ad.Append("; ").Append(layerBreakdown);
                ce.AdditionalData = ad.ToString();

                // keep zero-volume rows but leave them out of the calculation rather than corrupting totals
                ce.includeInCalc = volume > 0;

                result.Add(ce);
            }

            if (noVolumeCount > 0)
                Warnings.Add(noVolumeCount + " element(s) had no volume quantity and were imported with includeInCalc = false. Re-export the IFC with base quantities enabled.");
            if (noTagCount > 0)
                Warnings.Add(noTagCount + " element(s) had no numeric Revit Tag; the IFC line id was used as a fallback Id.");
            if (result.Count == 0)
                Warnings.Add("No importable elements found. Check that ElementTypes covers what the IFC contains.");

            return result;
        }

        // ====================================================================================
        //  Material resolution
        // ====================================================================================

        private class MaterialResult
        {
            public string Name;
            public string Category;
            public string Breakdown;
        }

        private MaterialResult ResolveMaterial(IfcModel m, int id, Units units, HashSet<int> visited)
        {
            if (id <= 0 || !visited.Add(id)) return null;
            string t = m.TypeOf(id);
            if (t == null) return null;
            var a = m.Attrs(id);

            switch (t)
            {
                case "IFCMATERIAL":
                {
                    string name = AsString(GetAttr(a, 0));
                    string cat = a.Count > 2 ? AsString(GetAttr(a, 2)) : null; // IFC4: IfcMaterial.Category
                    return new MaterialResult { Name = name, Category = cat, Breakdown = null };
                }

                case "IFCMATERIALLAYERSETUSAGE":
                    return ResolveMaterial(m, GetRef(a, 0), units, visited);   // ForLayerSet

                case "IFCMATERIALPROFILESETUSAGE":
                    return ResolveMaterial(m, GetRef(a, 0), units, visited);   // ForProfileSet

                case "IFCMATERIALLAYERSET":
                {
                    var parts = new List<KeyValuePair<string, double>>(); // name, thickness(mm)
                    foreach (var layerId in GetRefs(a, 0))                 // MaterialLayers
                    {
                        var la = m.Attrs(layerId);
                        int subMat = GetRef(la, 0);                        // IfcMaterialLayer.Material
                        double thick; GetNum(la, 1, out thick);            // LayerThickness (length units)
                        thick = thick * units.LengthToM * 1000.0;          // -> mm for display
                        string nm = null;
                        if (subMat > 0)
                        {
                            var sub = ResolveMaterial(m, subMat, units, visited);
                            if (sub != null) nm = sub.Name;
                        }
                        if (string.IsNullOrEmpty(nm)) nm = AsString(GetAttr(la, 3)) ?? "Air/Unnamed"; // layer Name
                        parts.Add(new KeyValuePair<string, double>(nm, thick));
                    }
                    return SummariseParts(parts);
                }

                case "IFCMATERIALCONSTITUENTSET":
                {
                    var parts = new List<KeyValuePair<string, double>>();
                    foreach (var conId in GetRefs(a, 2))                   // MaterialConstituents
                    {
                        var ca = m.Attrs(conId);
                        var sub = ResolveMaterial(m, GetRef(ca, 2), units, visited); // IfcMaterialConstituent.Material
                        string nm = sub != null ? sub.Name : AsString(GetAttr(ca, 0));
                        parts.Add(new KeyValuePair<string, double>(nm ?? "Unnamed", 0));
                    }
                    return SummariseParts(parts);
                }

                case "IFCMATERIALPROFILESET":
                {
                    var parts = new List<KeyValuePair<string, double>>();
                    foreach (var profId in GetRefs(a, 2))                  // MaterialProfiles
                    {
                        var pa = m.Attrs(profId);
                        var sub = ResolveMaterial(m, GetRef(pa, 3), units, visited); // IfcMaterialProfile.Material
                        string nm = sub != null ? sub.Name : AsString(GetAttr(pa, 0));
                        parts.Add(new KeyValuePair<string, double>(nm ?? "Unnamed", 0));
                    }
                    return SummariseParts(parts);
                }

                case "IFCMATERIALLIST":                                    // IFC2x3
                {
                    var parts = new List<KeyValuePair<string, double>>();
                    foreach (var matId in GetRefs(a, 0))                   // Materials
                    {
                        var sub = ResolveMaterial(m, matId, units, visited);
                        if (sub != null) parts.Add(new KeyValuePair<string, double>(sub.Name ?? "Unnamed", 0));
                    }
                    return SummariseParts(parts);
                }

                case "IFCMATERIALLAYER":
                    return ResolveMaterial(m, GetRef(a, 0), units, visited);
                case "IFCMATERIALCONSTITUENT":
                    return ResolveMaterial(m, GetRef(a, 2), units, visited);
                case "IFCMATERIALPROFILE":
                    return ResolveMaterial(m, GetRef(a, 3), units, visited);

                default:
                {
                    string name = AsString(GetAttr(a, 0));
                    return name != null ? new MaterialResult { Name = name } : null;
                }
            }
        }

        private static MaterialResult SummariseParts(List<KeyValuePair<string, double>> parts)
        {
            if (parts.Count == 0) return null;

            // primary = thickest layer if thicknesses are known, otherwise the first part
            KeyValuePair<string, double> primary = parts[0];
            bool haveThickness = parts.Any(p => p.Value > 0);
            if (haveThickness)
                primary = parts.OrderByDescending(p => p.Value).First();

            var sb = new StringBuilder("Layers: ");
            for (int i = 0; i < parts.Count; i++)
            {
                if (i > 0) sb.Append(" | ");
                sb.Append(parts[i].Key);
                if (parts[i].Value > 0)
                    sb.Append(' ').Append(parts[i].Value.ToString("0.#", CultureInfo.InvariantCulture)).Append("mm");
            }

            return new MaterialResult { Name = primary.Key, Breakdown = sb.ToString() };
        }

        // ====================================================================================
        //  Quantities + grade
        // ====================================================================================

        private static readonly string[] VolumePriority = { "NETVOLUME", "GROSSVOLUME" };
        private static readonly string[] AreaPriority =
            { "NETSIDEAREA", "NETAREA", "GROSSAREA", "NETFLOORAREA", "GROSSFLOORAREA", "OUTERSURFACEAREA", "TOTALSURFACEAREA", "GROSSSIDEAREA" };
        private static readonly string[] GradeKeys =
            { "GRADE", "STRENGTHCLASS", "CONCRETEGRADE", "STEELGRADE", "MATERIALGRADE", "STRENGTH" };

        private void ExtractFromPropertyDefs(IfcModel m, List<int> defIds, Units units,
                                             ref double volume, ref double area, ref string grade)
        {
            double bestVol = 0, bestArea = 0;
            int bestVolRank = int.MaxValue, bestAreaRank = int.MaxValue;

            foreach (var defId in defIds)
            {
                string t = m.TypeOf(defId);
                if (t == "IFCELEMENTQUANTITY")
                {
                    var a = m.Attrs(defId);
                    foreach (var qId in GetRefs(a, 5))     // Quantities
                    {
                        string qt = m.TypeOf(qId);
                        var qa = m.Attrs(qId);
                        string qname = (AsString(GetAttr(qa, 0)) ?? "").Replace(" ", "").ToUpperInvariant();

                        if (qt == "IFCQUANTITYVOLUME")
                        {
                            double v; if (!GetNum(qa, 3, out v)) continue;
                            int rank = RankOf(qname, VolumePriority);
                            if (rank < bestVolRank) { bestVolRank = rank; bestVol = v * units.VolumeToM3; }
                        }
                        else if (qt == "IFCQUANTITYAREA")
                        {
                            double v; if (!GetNum(qa, 3, out v)) continue;
                            int rank = RankOf(qname, AreaPriority);
                            if (rank < bestAreaRank) { bestAreaRank = rank; bestArea = v * units.AreaToM2; }
                        }
                    }
                }
                else if (t == "IFCPROPERTYSET" && grade == null)
                {
                    var a = m.Attrs(defId);
                    foreach (var pId in GetRefs(a, 4))     // HasProperties
                    {
                        if (m.TypeOf(pId) != "IFCPROPERTYSINGLEVALUE") continue;
                        var pa = m.Attrs(pId);
                        string pname = (AsString(GetAttr(pa, 0)) ?? "").Replace(" ", "").ToUpperInvariant();
                        if (GradeKeys.Any(k => pname.Contains(k)))
                        {
                            string val = AsString(GetAttr(pa, 2)); // NominalValue (typically wrapped, e.g. IFCLABEL('C32/40'))
                            if (!string.IsNullOrEmpty(val)) { grade = val; break; }
                        }
                    }
                }
            }

            if (bestVol > 0) volume = bestVol;
            if (bestArea > 0) area = bestArea;
        }

        private static int RankOf(string name, string[] priority)
        {
            for (int i = 0; i < priority.Length; i++)
                if (name == priority[i]) return i;
            return priority.Length; // unknown but still a valid quantity of the right kind
        }

        // ====================================================================================
        //  Storey
        // ====================================================================================

        private class Storey { public double Elevation; public string Name; }

        private Storey ReadStorey(IfcModel m, int id, Units units)
        {
            var a = m.Attrs(id);
            string name = AsString(GetAttr(a, 2));
            if (string.IsNullOrEmpty(name)) name = AsString(GetAttr(a, 8)); // LongName
            double elev; GetNum(a, 9, out elev);                            // Elevation (length units)
            return new Storey { Name = name, Elevation = elev * units.LengthToM };
        }

        // ====================================================================================
        //  Units
        // ====================================================================================

        private class Units
        {
            public double LengthToM = 1.0;
            public double AreaToM2 = 1.0;
            public double VolumeToM3 = 1.0;
        }

        private Units ParseUnits(IfcModel m)
        {
            var u = new Units();
            bool areaSet = false, volumeSet = false;

            int projId = m.AllIds.FirstOrDefault(i => m.TypeOf(i) == "IFCPROJECT");
            if (projId == 0) { Warnings.Add("No IfcProject found; assuming SI metres."); return u; }

            int uaId = GetRef(m.Attrs(projId), 8);   // UnitsInContext
            if (uaId <= 0 || m.TypeOf(uaId) != "IFCUNITASSIGNMENT") { Warnings.Add("No IfcUnitAssignment found; assuming SI metres."); return u; }

            foreach (var unitId in GetRefs(m.Attrs(uaId), 0))
            {
                string t = m.TypeOf(unitId);
                var a = m.Attrs(unitId);

                if (t == "IFCSIUNIT")
                {
                    string unitType = AsEnum(GetAttr(a, 1));
                    double prefix = PrefixFactor(AsEnum(GetAttr(a, 2)));
                    if (unitType == "LENGTHUNIT") u.LengthToM = prefix;          // metre base = 1
                    else if (unitType == "AREAUNIT") { u.AreaToM2 = prefix; areaSet = true; }
                    else if (unitType == "VOLUMEUNIT") { u.VolumeToM3 = prefix; volumeSet = true; }
                }
                else if (t == "IFCCONVERSIONBASEDUNIT")
                {
                    string unitType = AsEnum(GetAttr(a, 1));
                    int mwuId = GetRef(a, 3);                                     // ConversionFactor -> IfcMeasureWithUnit
                    double factor = 1.0;
                    if (mwuId > 0)
                    {
                        var mwu = m.Attrs(mwuId);
                        GetNum(mwu, 0, out factor);                              // ValueComponent (SI-per-unit)
                    }
                    if (unitType == "LENGTHUNIT") u.LengthToM = factor;
                    else if (unitType == "AREAUNIT") { u.AreaToM2 = factor; areaSet = true; }
                    else if (unitType == "VOLUMEUNIT") { u.VolumeToM3 = factor; volumeSet = true; }
                }
            }

            // derive area / volume from length where they weren't explicitly declared
            if (!areaSet) u.AreaToM2 = u.LengthToM * u.LengthToM;
            if (!volumeSet) u.VolumeToM3 = u.LengthToM * u.LengthToM * u.LengthToM;
            return u;
        }

        private static double PrefixFactor(string prefix)
        {
            switch (prefix)
            {
                case "EXA": return 1e18;
                case "PETA": return 1e15;
                case "TERA": return 1e12;
                case "GIGA": return 1e9;
                case "MEGA": return 1e6;
                case "KILO": return 1e3;
                case "HECTO": return 1e2;
                case "DECA": return 1e1;
                case "DECI": return 1e-1;
                case "CENTI": return 1e-2;
                case "MILLI": return 1e-3;
                case "MICRO": return 1e-6;
                case "NANO": return 1e-9;
                default: return 1.0;
            }
        }

        // ====================================================================================
        //  Friendly type names
        // ====================================================================================

        private static string FriendlyType(string ifcType)
        {
            switch (ifcType)
            {
                case "IFCWALL":
                case "IFCWALLSTANDARDCASE": return "Wall";
                case "IFCSLAB":
                case "IFCSLABSTANDARDCASE":
                case "IFCSLABELEMENTEDCASE": return "Slab";
                case "IFCBEAM":
                case "IFCBEAMSTANDARDCASE": return "Beam";
                case "IFCCOLUMN":
                case "IFCCOLUMNSTANDARDCASE": return "Column";
                case "IFCMEMBER":
                case "IFCMEMBERSTANDARDCASE": return "Member";
                case "IFCPLATE":
                case "IFCPLATESTANDARDCASE": return "Plate";
                case "IFCFOOTING": return "Footing";
                case "IFCPILE": return "Pile";
                case "IFCREINFORCINGBAR": return "Rebar";
                case "IFCREINFORCINGMESH": return "Mesh";
                case "IFCTENDON": return "Tendon";
                case "IFCBUILDINGELEMENTPROXY": return "Generic";
                case "IFCCURTAINWALL": return "Curtain Wall";
                default:
                    string s = ifcType.StartsWith("IFC", StringComparison.OrdinalIgnoreCase) ? ifcType.Substring(3) : ifcType;
                    if (s.EndsWith("STANDARDCASE", StringComparison.OrdinalIgnoreCase)) s = s.Substring(0, s.Length - 12);
                    return char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
            }
        }

        // ====================================================================================
        //  Attribute accessors (work on the parsed object list of an instance)
        // ====================================================================================

        private static object GetAttr(List<object> attrs, int i)
        {
            return (attrs != null && i >= 0 && i < attrs.Count) ? attrs[i] : null;
        }

        private static string AsString(object o)
        {
            if (o == null) return null;
            var s = o as string; if (s != null) return s;
            var ty = o as IfcTyped; if (ty != null && ty.Args != null && ty.Args.Count > 0) return AsString(ty.Args[0]);
            var en = o as IfcEnum; if (en != null) return en.Value;
            if (o is double) return ((double)o).ToString(CultureInfo.InvariantCulture);
            return null;
        }

        private static string AsEnum(object o)
        {
            var en = o as IfcEnum; if (en != null) return en.Value;
            return o as string;
        }

        private static int GetRef(List<object> attrs, int i)
        {
            var r = GetAttr(attrs, i) as IfcRef;
            return r != null ? r.Id : -1;
        }

        private static IEnumerable<int> GetRefs(List<object> attrs, int i)
        {
            var list = GetAttr(attrs, i) as List<object>;
            if (list == null) yield break;
            foreach (var o in list)
            {
                var r = o as IfcRef;
                if (r != null) yield return r.Id;
            }
        }

        private static bool GetNum(List<object> attrs, int i, out double value)
        {
            value = 0;
            object o = GetAttr(attrs, i);
            if (o == null) return false;
            if (o is double) { value = (double)o; return true; }
            var ty = o as IfcTyped; if (ty != null && ty.Args != null && ty.Args.Count > 0) return GetNumFromObj(ty.Args[0], out value);
            return GetNumFromObj(o, out value);
        }

        private static bool GetNumFromObj(object o, out double value)
        {
            value = 0;
            if (o is double) { value = (double)o; return true; }
            var s = o as string;
            if (s != null) return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            return false;
        }
    }

    // ========================================================================================
    //  Token types produced by the STEP tokeniser
    // ========================================================================================

    internal sealed class IfcRef { public int Id; public IfcRef(int id) { Id = id; } }
    internal sealed class IfcEnum { public string Value; public IfcEnum(string v) { Value = v; } }
    internal sealed class IfcTyped { public string Keyword; public List<object> Args; public IfcTyped(string k, List<object> a) { Keyword = k; Args = a; } }

    // ========================================================================================
    //  IfcModel - holds raw instances, parses attributes lazily, resolves the unit/header info
    // ========================================================================================

    internal sealed class IfcModel
    {
        private sealed class Raw { public string Type; public string Params; }

        private readonly Dictionary<int, Raw> _raw = new Dictionary<int, Raw>();
        private readonly Dictionary<int, List<object>> _cache = new Dictionary<int, List<object>>();

        public string Schema { get; private set; }
        public IEnumerable<int> AllIds { get { return _raw.Keys; } }

        public IfcModel(string text)
        {
            Schema = ExtractSchema(text);
            foreach (var stmt in ReadStatements(text))
                _raw[stmt.Item1] = new Raw { Type = stmt.Item2, Params = stmt.Item3 };
        }

        public string TypeOf(int id)
        {
            Raw r; return _raw.TryGetValue(id, out r) ? r.Type : null;
        }

        public List<object> Attrs(int id)
        {
            List<object> cached;
            if (_cache.TryGetValue(id, out cached)) return cached;
            Raw r;
            if (!_raw.TryGetValue(id, out r)) { return new List<object>(); }
            int pos = 0;
            var parsed = StepParser.ReadList(r.Params, ref pos);
            _cache[id] = parsed;
            return parsed;
        }

        private static string ExtractSchema(string text)
        {
            int i = text.IndexOf("FILE_SCHEMA", StringComparison.OrdinalIgnoreCase);
            if (i < 0) return null;
            int q1 = text.IndexOf('\'', i);
            if (q1 < 0) return null;
            int q2 = text.IndexOf('\'', q1 + 1);
            if (q2 < 0) return null;
            return text.Substring(q1 + 1, q2 - q1 - 1);
        }

        // Linear scan that splits the DATA section into (#id, TYPE, "(params)") statements.
        // Quote- and paren-aware so commas/semicolons inside strings and aggregates are safe.
        private static IEnumerable<Tuple<int, string, string>> ReadStatements(string text)
        {
            int end = text.Length;
            int dataAt = text.IndexOf("DATA;", StringComparison.OrdinalIgnoreCase);
            int i = dataAt >= 0 ? dataAt + 5 : 0;

            while (i < end)
            {
                while (i < end && text[i] != '#') i++;
                if (i >= end) yield break;

                i++; // skip '#'
                int idStart = i;
                while (i < end && char.IsDigit(text[i])) i++;
                if (i == idStart) continue;
                int id = int.Parse(text.Substring(idStart, i - idStart), CultureInfo.InvariantCulture);

                while (i < end && char.IsWhiteSpace(text[i])) i++;
                if (i >= end || text[i] != '=') continue;
                i++;
                while (i < end && char.IsWhiteSpace(text[i])) i++;

                int typeStart = i;
                while (i < end && (char.IsLetterOrDigit(text[i]) || text[i] == '_')) i++;
                if (i == typeStart) continue;
                string type = text.Substring(typeStart, i - typeStart).ToUpperInvariant();

                while (i < end && char.IsWhiteSpace(text[i])) i++;
                if (i >= end || text[i] != '(') continue;

                int paramStart = i;
                int depth = 0;
                bool inStr = false;
                for (; i < end; i++)
                {
                    char c = text[i];
                    if (inStr)
                    {
                        if (c == '\'')
                        {
                            if (i + 1 < end && text[i + 1] == '\'') { i++; continue; }
                            inStr = false;
                        }
                        continue;
                    }
                    if (c == '\'') { inStr = true; continue; }
                    if (c == '(') depth++;
                    else if (c == ')') { depth--; if (depth == 0) { i++; break; } }
                }
                string raw = text.Substring(paramStart, i - paramStart);

                yield return Tuple.Create(id, type, raw);

                while (i < end && text[i] != ';' && text[i] != '#') i++;
                if (i < end && text[i] == ';') i++;
            }
        }
    }

    // ========================================================================================
    //  StepParser - turns a "(...)" parameter string into a List<object> of token values
    // ========================================================================================

    internal static class StepParser
    {
        // s[pos] is expected to be '('. Reads a comma-separated value list up to the matching ')'.
        public static List<object> ReadList(string s, ref int pos)
        {
            var list = new List<object>();
            SkipWs(s, ref pos);
            if (pos >= s.Length || s[pos] != '(') return list;
            pos++; // consume '('
            SkipWs(s, ref pos);
            if (pos < s.Length && s[pos] == ')') { pos++; return list; }

            while (pos < s.Length)
            {
                list.Add(ReadValue(s, ref pos));
                SkipWs(s, ref pos);
                if (pos >= s.Length) break;
                char c = s[pos];
                if (c == ',') { pos++; continue; }
                if (c == ')') { pos++; break; }
                pos++; // defensive: skip anything unexpected to avoid stalling
            }
            return list;
        }

        private static object ReadValue(string s, ref int pos)
        {
            SkipWs(s, ref pos);
            if (pos >= s.Length) return null;
            char c = s[pos];

            if (c == '\'') return ReadString(s, ref pos);
            if (c == '#') return ReadRef(s, ref pos);
            if (c == '(') return ReadList(s, ref pos);
            if (c == '$') { pos++; return null; }
            if (c == '*') { pos++; return null; }            // derived value -> treat as unset
            if (c == '.') return ReadEnum(s, ref pos);
            if (c == '+' || c == '-' || char.IsDigit(c)) return ReadNumber(s, ref pos);
            if (char.IsLetter(c) || c == '_') return ReadKeyword(s, ref pos);

            pos++;
            return null;
        }

        private static void SkipWs(string s, ref int pos)
        {
            while (pos < s.Length && char.IsWhiteSpace(s[pos])) pos++;
        }

        private static object ReadRef(string s, ref int pos)
        {
            pos++; // '#'
            int start = pos;
            while (pos < s.Length && char.IsDigit(s[pos])) pos++;
            int id = (pos > start) ? int.Parse(s.Substring(start, pos - start), CultureInfo.InvariantCulture) : -1;
            return new IfcRef(id);
        }

        private static object ReadEnum(string s, ref int pos)
        {
            pos++; // opening '.'
            int start = pos;
            while (pos < s.Length && s[pos] != '.') pos++;
            string val = s.Substring(start, pos - start);
            if (pos < s.Length && s[pos] == '.') pos++; // closing '.'
            return new IfcEnum(val);
        }

        private static object ReadNumber(string s, ref int pos)
        {
            int start = pos;
            if (s[pos] == '+' || s[pos] == '-') pos++;
            while (pos < s.Length && char.IsDigit(s[pos])) pos++;
            if (pos < s.Length && s[pos] == '.') { pos++; while (pos < s.Length && char.IsDigit(s[pos])) pos++; }
            if (pos < s.Length && (s[pos] == 'e' || s[pos] == 'E'))
            {
                pos++;
                if (pos < s.Length && (s[pos] == '+' || s[pos] == '-')) pos++;
                while (pos < s.Length && char.IsDigit(s[pos])) pos++;
            }
            double d;
            double.TryParse(s.Substring(start, pos - start), NumberStyles.Any, CultureInfo.InvariantCulture, out d);
            return d;
        }

        private static object ReadKeyword(string s, ref int pos)
        {
            int start = pos;
            while (pos < s.Length && (char.IsLetterOrDigit(s[pos]) || s[pos] == '_')) pos++;
            string kw = s.Substring(start, pos - start);
            SkipWs(s, ref pos);
            if (pos < s.Length && s[pos] == '(')
            {
                var args = ReadList(s, ref pos);            // typed value, e.g. IFCLABEL('C32/40')
                return new IfcTyped(kw.ToUpperInvariant(), args);
            }
            return new IfcEnum(kw);                          // bare keyword / logical
        }

        private static object ReadString(string s, ref int pos)
        {
            pos++; // opening quote
            var sb = new StringBuilder();
            while (pos < s.Length)
            {
                char c = s[pos];
                if (c == '\'')
                {
                    if (pos + 1 < s.Length && s[pos + 1] == '\'') { sb.Append('\''); pos += 2; continue; }
                    pos++; break;
                }
                if (c == '\\')
                {
                    int consumed;
                    string dec = DecodeEscape(s, pos, out consumed);
                    if (consumed > 0) { sb.Append(dec); pos += consumed; continue; }
                }
                sb.Append(c);
                pos++;
            }
            return sb.ToString();
        }

        // Handles the common IFC string escapes: \X2\HHHH..\X0\ (UTF-16), \X\HH (single byte),
        // and \S\c (ISO 8859 high bit). Anything unrecognised falls through (consumed = 0).
        private static string DecodeEscape(string s, int i, out int consumed)
        {
            consumed = 0;
            if (i + 1 >= s.Length) return null;
            char n = s[i + 1];

            if ((n == 'X' || n == 'x') && i + 2 < s.Length)
            {
                if (s[i + 2] == '2')
                {
                    int j = i + 3;
                    if (j < s.Length && s[j] == '\\') j++;   // backslash after X2
                    var sb = new StringBuilder();
                    while (j + 3 < s.Length)
                    {
                        if (s[j] == '\\' && (s[j + 1] == 'X' || s[j + 1] == 'x') && s[j + 2] == '0' && s[j + 3] == '\\')
                        { j += 4; break; }
                        if (j + 4 <= s.Length && IsHex(s, j, 4))
                        {
                            sb.Append((char)Convert.ToInt32(s.Substring(j, 4), 16));
                            j += 4;
                        }
                        else break;
                    }
                    consumed = j - i;
                    return sb.ToString();
                }
                if (s[i + 2] == '\\' && i + 4 < s.Length && IsHex(s, i + 3, 2))
                {
                    char ch = (char)Convert.ToInt32(s.Substring(i + 3, 2), 16);
                    consumed = 5;
                    return ch.ToString();
                }
            }
            else if (n == 'S' && i + 3 < s.Length && s[i + 2] == '\\')
            {
                consumed = 4;
                return ((char)(s[i + 3] + 0x80)).ToString();
            }
            return null;
        }

        private static bool IsHex(string s, int start, int len)
        {
            if (start + len > s.Length) return false;
            for (int k = 0; k < len; k++)
                if (!Uri.IsHexDigit(s[start + k])) return false;
            return true;
        }
    }
}
