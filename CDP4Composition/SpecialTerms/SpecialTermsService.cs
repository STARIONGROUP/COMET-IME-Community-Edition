namespace CDP4Composition.Services
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;

    /// <summary>
    /// The purpose of the <see cref="SpecialTermsService"/> is to provide access to special terms to be
    /// processed by text boxes.
    /// </summary>
    [Export(typeof(ISpecialTermsService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SpecialTermsService : ISpecialTermsService
    {
        /// <summary>
        /// Backing field for <see cref="TermDefinitionDictionary"/>
        /// </summary>
        private Dictionary<string, Definition> termDefinitionDictionary = new Dictionary<string, Definition>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The session that is responsible for providing the special terms.
        /// </summary>
        private ISession dialogSession; 

        /// <summary>
        /// Gets the dictionary of terms and their definitions.
        /// </summary>
        public Dictionary<string, Definition> TermDefinitionDictionary
        {
            get { return this.termDefinitionDictionary; }
        }

        /// <summary>
        /// Gets the list of <see cref="Term"/> that need to be highlighted.
        /// </summary>
        public IEnumerable<string> TermsList
        {
            get { return this.termDefinitionDictionary.Keys; }
        }

        /// <summary>
        /// Creates the initial terms dictionary and instantiates the listeners to pick up changes.
        /// </summary>
        /// <param name="session">
        /// The session.
        /// </param>
        public void RebuildDictionaries(ISession session)
        {
            // reload all open sessions.
            this.dialogSession = session;

            // rebuild all terms
            this.RebuildTermDictionaries();
        }

        /// <summary>
        /// Rebuilds the <see cref="Term"/> dictionaries.
        /// </summary>
        private void RebuildTermDictionaries()
        {
            // site and model rdl
            var openDataLibrariesIids = this.dialogSession.OpenReferenceDataLibraries.Select(y => y.Iid);
            foreach (var siteReferenceDataLibrary in this.dialogSession.RetrieveSiteDirectory().AvailableReferenceDataLibraries().Where(x => openDataLibrariesIids.Contains(x.Iid)))
            {
                foreach (var glossary in siteReferenceDataLibrary.Glossary)
                {
                    foreach (var term in glossary.Term)
                    {
                        var definition = term.Definition.FirstOrDefault();

                        this.TermDefinitionDictionary[term.Name] = definition ?? new Definition(); 
                    }
                }
            }
        }
    }
}
