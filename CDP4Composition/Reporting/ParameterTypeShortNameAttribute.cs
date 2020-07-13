using System;

namespace CDP4Composition.Reporting
{
    public class ParameterTypeShortNameAttribute : Attribute
    {
        public ParameterTypeShortNameAttribute(string shortName)
        {
            this.ShortName = shortName;
        }

        public string ShortName { get; }
    }
}
