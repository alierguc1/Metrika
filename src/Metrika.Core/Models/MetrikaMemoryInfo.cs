using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrika.Core.Models
{
    /// <summary>
    /// Memory measurement information
    /// </summary>
    public class MetrikaMemoryInfo
    {
        /// <summary>
        /// Memory allocated/deallocated in bytes
        /// </summary>
        public long MemoryDelta { get; set; }

        /// <summary>
        /// Number of Gen0 garbage collections
        /// </summary>
        public int Gen0Collections { get; set; }

        /// <summary>
        /// Number of Gen1 garbage collections
        /// </summary>
        public int Gen1Collections { get; set; }

        /// <summary>
        /// Number of Gen2 garbage collections
        /// </summary>
        public int Gen2Collections { get; set; }

        /// <summary>
        /// Total number of garbage collections
        /// </summary>
        public int TotalCollections => Gen0Collections + Gen1Collections + Gen2Collections;

        /// <summary>
        /// Memory delta in megabytes
        /// </summary>
        public double MemoryDeltaMB => MemoryDelta / 1024.0 / 1024.0;

        /// <summary>
        /// Whether memory usage is considered high (&gt;100 MB allocated)
        /// </summary>
        public bool IsHighMemoryUsage => Math.Abs(MemoryDelta) > 100_000_000; // 100 MB

        /// <summary>
        /// Whether GC pressure is high (Gen2 collection occurred)
        /// </summary>
        public bool IsHighGCPressure => Gen2Collections > 0;
    }
}
