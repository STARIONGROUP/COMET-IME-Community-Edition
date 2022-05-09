using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDP4EngineeringModel.Converters
{
    using CDP4Common.SiteDirectoryData;

    using CDP4Composition.CommonView.ViewModels;
    using CDP4Composition.Converters;

    class ReactiveActiveDomainToObjectListConverter : GenericListToObjectListConverter<ActiveDomainRowViewModel>
    {
    }
}
