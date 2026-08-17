using Autodesk.Revit.DB;

namespace CarboLifeRevitCompat
{
    /// <summary>
    /// Bridges the one Revit API break that matters between the two builds.
    ///
    /// Revit 2024 widened element ids to 64 bit: it added <c>ElementId(Int64)</c> and
    /// <c>ElementId.Value</c> alongside the existing <c>ElementId(Int32)</c> /
    /// <c>ElementId.IntegerValue</c>. The NET8 build compiles against Revit 2025 and has
    /// both; the 4.8 build compiles against Revit 2023, which has neither.
    ///
    /// Ids are stored as <see cref="long"/> throughout CarboLifeAPI to match the newer
    /// API, so the narrowing happens only here, at the Revit boundary. Ids on Revit 2023
    /// always fit in an <see cref="int"/>, so nothing is lost.
    ///
    /// Compiling against 2023 still gives a binary that runs on 2024, because 2024 keeps
    /// the 32-bit members.
    ///
    /// This file lives in Shared\ and is linked into CarboLifeRevit and CarboCircle.
    /// </summary>
    internal static class RevitCompat
    {
        /// <summary>
        /// Replaces <c>new ElementId(someInt64)</c>.
        /// </summary>
        public static ElementId ToElementId(this long id)
        {
#if NETFRAMEWORK
            //An id that cannot be represented in 32 bits cannot exist in this Revit
            //version, so it can only have come from a project saved by a newer one.
            //Hand back the invalid id rather than throwing: callers already treat a
            //missing element as "skip", and an id from another model would not have
            //resolved anyway.
            if (id < int.MinValue || id > int.MaxValue)
                return ElementId.InvalidElementId;

            return new ElementId((int)id);
#else
            return new ElementId(id);
#endif
        }

        /// <summary>
        /// Replaces <c>someElementId.Value</c>.
        /// </summary>
        public static long LongValue(this ElementId id)
        {
#if NETFRAMEWORK
            return id.IntegerValue;
#else
            return id.Value;
#endif
        }
    }
}
