using System.Collections.Generic;

namespace LocaConstants {
    /// <summary>
    /// We don't need to convert string to int in this special case, so we save time on deserilization by not having to convert from the serilaized string value into int
    /// </summary>
    public class GeneratorLocaModel {
        public Dictionary<string, string> translations { get; set;}
    }
}