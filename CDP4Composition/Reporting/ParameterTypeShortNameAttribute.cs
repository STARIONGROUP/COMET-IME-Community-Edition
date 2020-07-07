namespace CDP4Composition.Reporting
{
    using System;

    public class ParameterTypeShortNameAttribute : Attribute
    {
        // TODO better way to identify parameters, full name and maybe RDL?
        public string ShortName { get; }

        public ParameterTypeShortNameAttribute(string shortName)
        {
            this.ShortName = shortName;
        }
    }
}
