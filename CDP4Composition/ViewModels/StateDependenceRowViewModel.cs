
using System.Text;
using System.Threading.Tasks;

namespace CDP4Composition.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Types;

    /// <summary>
    /// Represents an element parameter to use in <see cref="ParameterRowControlViewModel"/> in the grapher
    /// </summary>
    public class StateDependenceRowViewModel : ParameterRowControlViewModel
    {
        /// <summary>
        /// Gets or sets the <see cref="ContainerList{T}"/> of <see cref="ActualFiniteStates"/>
        /// </summary>
        public ContainerList<ActualFiniteState> ActualFiniteStates { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IEnumerable{T}"/> of <see cref="IValueSet"/>
        /// </summary>
        public IEnumerable<IValueSet> ValueSets { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="StateDependenceRowViewModel"/>
        /// </summary>
        /// <param name="actualFiniteStates">The <see cref="ContainerList{T}"/> of <see cref="ActualFiniteStates"/></param>
        /// <param name="valueSets">The <see cref="IEnumerable{T}"/> of <see cref="IValueSet"/></param>
        public StateDependenceRowViewModel(ContainerList<ActualFiniteState> actualFiniteStates, IEnumerable<IValueSet> valueSets)
        {
            this.ActualFiniteStates = actualFiniteStates;
            this.ValueSets = valueSets;
        }

        /// <summary>
        /// Generates a list of <see cref="ParameterRowControlViewModel"/>
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of <see cref="ParameterRowControlViewModel"/></returns>
        public List<ParameterRowControlViewModel> GenerateStateRows()
        {
            var rowList = new List<ParameterRowControlViewModel>();

            foreach (var state in this.ActualFiniteStates)
            {
                var row = this.GenerateViewModelRow(state);
                rowList.Add(row);
            }

            return rowList;
        }

        /// <summary>
        /// Generates a row of <see cref="ParameterRowControlViewModel"/> based on the current <see cref="ActualFiniteState"/>
        /// </summary>
        /// <param name="actualFiniteState">The current <see cref="ActualFiniteState"/></param>
        /// <returns></returns>
        private ParameterRowControlViewModel GenerateViewModelRow(ActualFiniteState actualFiniteState)
        {
            var valueSet = this.GetValueSet(actualFiniteState.Iid);
            var actualValue = "-";
            var publishedValue = "-";

            if (valueSet is ParameterValueSet parameterValueSet)
            {
                actualValue = this.FormatValueString(parameterValueSet.ActualValue);
                publishedValue = this.FormatValueString(parameterValueSet.Published);
            }
            
            var row = new ParameterRowControlViewModel
            {
                Name = actualFiniteState.Name,
                ShortName = actualFiniteState.ShortName,
                OwnerShortName = actualFiniteState.Owner?.ShortName,
                RowType = actualFiniteState.ClassKind.ToString(),
                Switch = valueSet.ValueSwitch.ToString(),
                ActualValue = actualValue,
                PublishedValue = publishedValue
            };

            return row;
        }

        /// <summary>
        /// Gets the matching <see cref="IValueSet"/> for a given <see cref="ActualFiniteState"/> Iid
        /// </summary>
        /// <param name="iid">The <see cref="Guid"/> for the current <see cref="ActualFiniteState"/></param>
        /// <returns></returns>
        private IValueSet GetValueSet(Guid iid)
        {
            var valueSet = this.ValueSets.FirstOrDefault(x => x.ActualState.Iid == iid);
            return valueSet;
        }
    }
}
