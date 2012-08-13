namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// StringEnumConverter that camel-case any text.
    /// </summary>
    public class CamelCaseStringEnumConverter : StringEnumConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCaseStringEnumConverter"/> class.
        /// </summary>
        public CamelCaseStringEnumConverter()
        {
            CamelCaseText = true;
        }
    }
}
