using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrika.Core.Models
{
    /// <summary>
    /// Localization strings for Metrika output
    /// </summary>
    public class MetrikaLocalization
    {
        /// <summary>
        /// Text shown when duration exceeds threshold (e.g., "duration high", "süresi yüksek")
        /// </summary>
        public string DurationHigh { get; set; } = "duration high";

        /// <summary>
        /// Text shown for normal duration (e.g., "duration", "süresi")
        /// </summary>
        public string Duration { get; set; } = "duration";

        /// <summary>
        /// Text shown for total duration in multi-step operations (e.g., "Total duration", "Toplam süre")
        /// </summary>
        public string TotalDuration { get; set; } = "Total duration";

        /// <summary>
        /// Prefix for all console output (e.g., "METRIKA", "METRİKA")
        /// </summary>
        public string Prefix { get; set; } = "METRIKA";

        /// <summary>
        /// Text shown for memory allocation (e.g., "Memory", "Bellek", "Mémoire")
        /// </summary>
        public string Memory { get; set; } = "Memory";

        /// <summary>
        /// Text shown for garbage collection info (usually kept as "GC" in all languages)
        /// </summary>
        public string GarbageCollection { get; set; } = "GC";

        /// <summary>
        /// Warning text for high memory usage (e.g., "HIGH MEMORY", "YÜKSEK BELLEK")
        /// </summary>
        public string HighMemory { get; set; } = "HIGH MEMORY";

        /// <summary>
        /// Warning text for GC pressure (e.g., "GC PRESSURE", "GC BASKISI")
        /// </summary>
        public string GCPressure { get; set; } = "GC PRESSURE";

        /// <summary>
        /// Default English localization
        /// </summary>
        public static MetrikaLocalization English => new()
        {
            DurationHigh = "duration high",
            Duration = "duration",
            TotalDuration = "Total duration",
            Prefix = "METRIKA",
            Memory = "Memory",
            GarbageCollection = "GC",
            HighMemory = "HIGH MEMORY",
            GCPressure = "GC PRESSURE"
        };

        /// <summary>
        /// Turkish localization
        /// </summary>
        public static MetrikaLocalization Turkish => new()
        {
            DurationHigh = "süresi yüksek",
            Duration = "süresi",
            TotalDuration = "Toplam süre",
            Prefix = "METRİKA",
            Memory = "Bellek",
            GarbageCollection = "GC",
            HighMemory = "YÜKSEK BELLEK",
            GCPressure = "GC BASKISI"
        };

        /// <summary>
        /// French localization
        /// </summary>
        public static MetrikaLocalization French => new()
        {
            DurationHigh = "durée élevée",
            Duration = "durée",
            TotalDuration = "Durée totale",
            Prefix = "METRIKA",
            Memory = "Mémoire",
            GarbageCollection = "GC",
            HighMemory = "MÉMOIRE ÉLEVÉE",
            GCPressure = "PRESSION GC"
        };

        /// <summary>
        /// German localization
        /// </summary>
        public static MetrikaLocalization German => new()
        {
            DurationHigh = "Dauer hoch",
            Duration = "Dauer",
            TotalDuration = "Gesamtdauer",
            Prefix = "METRIKA",
            Memory = "Speicher",
            GarbageCollection = "GC",
            HighMemory = "HOHER SPEICHER",
            GCPressure = "GC-DRUCK"
        };

        /// <summary>
        /// Spanish localization
        /// </summary>
        public static MetrikaLocalization Spanish => new()
        {
            DurationHigh = "duración alta",
            Duration = "duración",
            TotalDuration = "Duración total",
            Prefix = "METRIKA",
            Memory = "Memoria",
            GarbageCollection = "GC",
            HighMemory = "MEMORIA ALTA",
            GCPressure = "PRESIÓN GC"
        };

        /// <summary>
        /// Japanese localization
        /// </summary>
        public static MetrikaLocalization Japanese => new()
        {
            DurationHigh = "処理時間が長い",
            Duration = "処理時間",
            TotalDuration = "合計時間",
            Prefix = "メトリカ",
            Memory = "メモリ",
            GarbageCollection = "GC",
            HighMemory = "高メモリ使用",
            GCPressure = "GC圧力"
        };

        /// <summary>
        /// Chinese (Simplified) localization
        /// </summary>
        public static MetrikaLocalization ChineseSimplified => new()
        {
            DurationHigh = "持续时间长",
            Duration = "持续时间",
            TotalDuration = "总持续时间",
            Prefix = "指标",
            Memory = "内存",
            GarbageCollection = "GC",
            HighMemory = "高内存",
            GCPressure = "GC压力"
        };

        /// <summary>
        /// Russian localization
        /// </summary>
        public static MetrikaLocalization Russian => new()
        {
            DurationHigh = "длительность высокая",
            Duration = "длительность",
            TotalDuration = "Общая продолжительность",
            Prefix = "МЕТРИКА",
            Memory = "Память",
            GarbageCollection = "GC",
            HighMemory = "ВЫСОКАЯ ПАМЯТЬ",
            GCPressure = "ДАВЛЕНИЕ GC"
        };

        /// <summary>
        /// Portuguese localization
        /// </summary>
        public static MetrikaLocalization Portuguese => new()
        {
            DurationHigh = "duração alta",
            Duration = "duração",
            TotalDuration = "Duração total",
            Prefix = "METRIKA",
            Memory = "Memória",
            GarbageCollection = "GC",
            HighMemory = "MEMÓRIA ALTA",
            GCPressure = "PRESSÃO GC"
        };

        /// <summary>
        /// Italian localization
        /// </summary>
        public static MetrikaLocalization Italian => new()
        {
            DurationHigh = "durata elevata",
            Duration = "durata",
            TotalDuration = "Durata totale",
            Prefix = "METRIKA",
            Memory = "Memoria",
            GarbageCollection = "GC",
            HighMemory = "MEMORIA ALTA",
            GCPressure = "PRESSIONE GC"
        };
    }
}