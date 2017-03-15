using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;

namespace Flogging.Core
{
    class FloggerJsonFormatter : CustomElasticsearchJsonFormatter
    {
        private readonly string _propertyNameToBeTreatedAsSource;


        public FloggerJsonFormatter(string propertyNameToBeTreatedAsSource)
            : base(renderMessage: false, inlineFields: true)
        {
            _propertyNameToBeTreatedAsSource = propertyNameToBeTreatedAsSource.ToLowerInvariant();
        }

        /// <summary>
        /// Writes the _propertyNameToBeTreatedAsSource property as the _source for Object and Dictionary types
        /// This will behave exactly as ElasticsearchJsonFormatter if _propertyNameToBeTreatedAsSource is NOT specified
        /// </summary>
        protected override void WriteProperties(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
        {
            var precedingDelimiter = "";
            foreach (var property in properties)
            {
                if (property.Key.ToLowerInvariant() == _propertyNameToBeTreatedAsSource)
                {
                    if (property.Value is DictionaryValue)
                        WriteDictionaryWithoutWrapping(((DictionaryValue)property.Value).Elements, output);
                    else if (property.Value is StructureValue)
                        WriteStructureWithoutWrapping(((StructureValue)property.Value).Properties, output);
                    else
                        WriteJsonProperty(property.Key, property.Value, ref precedingDelimiter, output);
                }
                else
                    WriteJsonProperty(property.Key, property.Value, ref precedingDelimiter, output);
            }
        }
        /// <summary>
        /// Writes out the Structure without Wrapping in any top level object
        /// </summary>
        protected void WriteStructureWithoutWrapping(IEnumerable<LogEventProperty> properties, TextWriter output)
        {
            var delim = "";

            foreach (var property in properties)
                if (property.Name == "Timestamp")
                    WriteJsonProperty("@timestamp", property.Value, ref delim, output);
                else
                    WriteJsonProperty(property.Name, property.Value, ref delim, output);
        }

        /// <summary>
        /// Writes out the Dictionary without Wrapping in any top level object
        /// </summary>
        protected void WriteDictionaryWithoutWrapping(IReadOnlyDictionary<ScalarValue, LogEventPropertyValue> elements, TextWriter output)
        {
            var delim = "";

            foreach (var e in elements)
                WriteJsonProperty(e.Key.Value.ToString(), e.Value, ref delim, output);
        }

        /// <summary>
        /// Writes out the Structure without any other extra properties like _typeTag etc.
        /// </summary>
        protected override void WriteStructure(string typeTag, IEnumerable<LogEventProperty> properties, TextWriter output)
        {
            output.Write("{");

            var delim = "";

            foreach (var property in properties)
                WriteJsonProperty(property.Name, property.Value, ref delim, output);

            output.Write("}");
        }

        /// <summary>
        /// Do not write any extra message
        /// </summary>
        protected override void WriteRenderedMessage(string message, ref string delim, TextWriter output)
        {
            // do nothing
        }

        /// <summary>
        /// Do not write any message template
        /// </summary>
        protected override void WriteMessageTemplate(string template, ref string delim, TextWriter output)
        {
            // do nothing
        }

        /// <summary>
        /// Do not write any extra level as it is likely part of the destructed logged object already
        /// </summary>
        protected override void WriteLevel(LogEventLevel level, ref string delim, TextWriter output)
        {
            // do nothing
        }

        /// <summary>
        /// Do not write any extra timestamp as it is likely part of the destructed logged object already
        /// </summary>
        protected override void WriteTimestamp(DateTimeOffset timestamp, ref string delim, TextWriter output)
        {
            // do nothing

        }
    }

}
