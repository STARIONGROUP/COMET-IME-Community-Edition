using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDP4Composition.CommonView.ViewModels
{
    using CDP4Common.SiteDirectoryData;

    using ReactiveUI;

    public class ActiveDomainRowViewModel : ReactiveObject
    {
        private bool isVisible;

        public ActiveDomainRowViewModel(DomainOfExpertise domainOfExpertise, bool isVisible)
        {
            this.DomainOfExpertise = domainOfExpertise;
            this.IsVisible = isVisible;
        }

        public bool IsVisible
        {
            get => this.isVisible;
            private set => this.RaiseAndSetIfChanged(ref this.isVisible, value);
        }

        public DomainOfExpertise DomainOfExpertise { get; private set; }
    }
}
