// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScriptingProxy.cs" company="RHEA System S.A.">
//    Copyright (c) 2015-2020 RHEA System S.A.
//
//    Author: Sam Gerené, Alex Vorobiev, Merlin Bieze, Naron Phou, Patxi Ozkoidi, Alexander van Delft, Mihail Militaru
//            Nathanael Smiechowski, Kamil Wojnowski
//
//    This file is part of CDP4-IME Community Edition. 
//    The CDP4-IME Community Edition is the RHEA Concurrent Design Desktop Application and Excel Integration
//    compliant with ECSS-E-TM-10-25 Annex A and Annex C.
//
//    The CDP4-IME Community Edition is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Affero General Public
//    License as published by the Free Software Foundation; either
//    version 3 of the License, or any later version.
//
//    The CDP4-IME Community Edition is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Threading;
    using Attributes;
    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Composition.Navigation;
    using CDP4Composition.Navigation.Interfaces;
    using CDP4Dal;
    using ICSharpCode.AvalonEdit.CodeCompletion;
    using Interfaces;
    using IronPython.Modules;
    using ViewModels;

    /// <summary>
    /// The purpose of the <see cref="ScriptingProxy"/> class is to provide access to an <see cref="EngineeringModel"/>
    /// </summary>
    [Export(typeof(IScriptingProxy)), PartCreationPolicy(CreationPolicy.Shared)]
    public class ScriptingProxy : IScriptingProxy
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptingProxy"/> class.
        /// </summary>
        /// <param name="thingDialogNavigationService">The (MEF injected) instance of <see cref="IThingDialogNavigationService"/>.</param>
        /// <param name="panelNavigationService">The (MEF injected) instance of <see cref="IPanelNavigationService"/>.</param>
        /// <param name="dialogNavigationService">The (MEF injected) instance of <see cref="IDialogNavigationService"/>.</param>
        [ImportingConstructor]
        public ScriptingProxy(IThingDialogNavigationService thingDialogNavigationService, IPanelNavigationService panelNavigationService, IDialogNavigationService dialogNavigationService)
        {
            this.ThingDialogNavigationService = thingDialogNavigationService;
            this.PanelNavigationService = panelNavigationService;
            this.DialogNavigationService = dialogNavigationService;
            this.NestedElementTreeGenerator = new NestedElementTreeGenerator();
            this.InitCommandCompletionData();
        }

        /// <summary>
        /// Gets the <see cref="IPanelNavigationService"/> used to navigate to Panels.
        /// </summary>
        internal IPanelNavigationService PanelNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDialogNavigationService"/> that is used to support panel navigation.
        /// </summary>
        internal IDialogNavigationService DialogNavigationService { get; private set; }

        /// <summary>
        /// Gets the <see cref="IThingDialogNavigationService"/> that handles navigation to dialogs.
        /// </summary>
        internal IThingDialogNavigationService ThingDialogNavigationService { get; private set; }

        /// <summary>
        /// Gets or sets the view-model which calls the method of this class. 
        /// </summary>
        public IScriptPanelViewModel ScriptingPanelViewModel { get; set; }

        /// <summary>
        /// Gets or sets the list of the command completion data.
        /// </summary>
        public IList<ICompletionData> CommandCompletionData { get; private set; }

        /// <summary>
        /// The <see cref="CDP4Common.Helpers.NestedElementTreeGenerator"/> object that the user can use. 
        /// </summary>
        [Documentation("NestedElementTreeGenerator", "Allows the usage of the methods of this class.")]
        public NestedElementTreeGenerator NestedElementTreeGenerator { get; private set; }

        #region CDP4Commands
        /// <summary>
        /// A command to clear the output.
        /// </summary>
        [Documentation("Clear()", "Clears the output screen.")]
        public void Clear()
        {
            if (this.ScriptingPanelViewModel == null)
            {
                return;
            }

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(() => this.ScriptingPanelViewModel.OutputTerminal.Document.Blocks.Clear()));
        }

        /// <summary>
        /// Opens the model browser if a session is active. 
        /// </summary>
        public void OpenPanelModelBrowser()
        {
            if (this.ScriptingPanelViewModel?.SelectedSession != null)
            {
                this.PanelNavigationService.Open("Model Browser", this.ScriptingPanelViewModel.SelectedSession, true, this.ThingDialogNavigationService, this.DialogNavigationService);
            }
        }

        /// <summary>
        /// Opens the dialog associated to the <see cref="_ast.Name"/> if the <see cref="dialogName"/> matches.
        /// </summary>
        /// <param name="dialogName">The dialog name entered by the user in the script.</param>
        [Documentation("OpenDialog(string dialogName)", "Opens a dialog using its name.")]
        public void OpenDialog(string dialogName)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal,
                new Action(() => this.DialogNavigationService.NavigateModal(dialogName)));
        }

        /// <summary>
        /// Displays all the commands possible and explains how to use it.
        /// The commands available are the methods decorated with a <see cref="DocumentationAttribute"/>.
        /// It explains also how to get the data of an engineering model.
        /// </summary>
        [Documentation("Help()", "Displays the help on the output screen.")]
        public void Help()
        {
            if (this.ScriptingPanelViewModel == null)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("CDP4 Commands \n");

            sb.AppendLine(string.Format("CDP4 contains a list of commands you can perform from the script. You can use a command as follows : " +
                              "{0}.CommandName(parameters) \n", ScriptPanelViewModel.Command));

            sb.AppendLine("List of the commands available \n");

            var methods = typeof(ScriptingProxy).GetMethods();

            var properties = typeof(ScriptingProxy).GetProperties();
            
            foreach (var property in properties)
            {
                var attrs = property.GetCustomAttributes(typeof(DocumentationAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                foreach (var attr in attrs)
                {
                    var cmdAttr = attr as DocumentationAttribute;
                    if (cmdAttr?.Name != null && cmdAttr.Description != null)
                    {
                        sb.AppendLine(String.Format("{0} : {1}", cmdAttr.Name, cmdAttr.Description));
                    }
                }
            }

            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes(typeof(DocumentationAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                foreach (var attr in attrs)
                {
                    var cmdAttr = attr as DocumentationAttribute;
                    if (cmdAttr?.Name != null && cmdAttr.Description != null)
                    {
                        sb.AppendLine(String.Format("{0} : {1}", cmdAttr.Name, cmdAttr.Description));
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Input,
                        new Action(() => this.ScriptingPanelViewModel.OutputTerminal.AppendText(sb.ToString())));
        }
        #endregion

        #region DataCommands
        /// <summary>
        /// Clears the variables of the scope of the script.
        /// </summary>
        [Documentation("ClearVariables()", "Clears the variables of the scope of the script.")]
        public void ClearVariables()
        {
            if (this.ScriptingPanelViewModel.GetType() == typeof(PythonScriptPanelViewModel) || this.ScriptingPanelViewModel.GetType() == typeof(LuaScriptPanelViewModel))
            {
                this.ScriptingPanelViewModel.ClearScopeVariables();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(
                        DispatcherPriority.Input,
                        new Action(() => this.ScriptingPanelViewModel.OutputTerminal.AppendText("\nNot supported for this language")));
            }
        }

        /// <summary>
        /// Gets the engineering model data 
        /// </summary>
        /// <param name="texteditorCommand">
        /// The string to be given to search through the session 
        /// </param>
        /// <returns>
        /// A string with a value from an engineering model
        /// </returns>
        [Documentation("ModelCode(string \"Name of the model (LOFT),Iteration number (1),Element name (FEEs),Element name .property (FEEs.mass_margin),value by field(published)\")",
            "Gets the engineering model data and return a value from an engineering model.\nA connection to a data source and an open model are required")]
        public string ModelCode(string texteditorCommand)
        {
            var splitCommand = texteditorCommand.Split(',');

            if (this.ScriptingPanelViewModel == null)
            {
                return null;
            }

            if (this.ScriptingPanelViewModel.SelectedSession == null)
            {
                Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Input,
                new Action(() =>
                    this.ScriptingPanelViewModel.OutputTerminal.AppendText("No session is selected to run the script. ModelCode cannot be executed.\n")));
                return null;
            }

            var data = this.ScriptingPanelViewModel.SelectedSession.Assembler.Cache.Select(x => x.Value)
                        .Where(lazy => lazy.Value.ClassKind == ClassKind.EngineeringModel)
                        .Select(lazy => lazy.Value)
                        .Cast<EngineeringModel>();
            try
            {
                var engineeringModel = this.FindEngineeringModel(data, splitCommand[0]);
                var iteration = this.FindIterationNumber(engineeringModel.Iteration, splitCommand[1]);
                var element = this.FindElement(iteration.Element, splitCommand[2]);
                var parameter = this.FindParameter(element.Parameter, splitCommand[3]);
                var value = this.FindValue(parameter.ValueSet, splitCommand[4]);

                return (value);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Input,
                    new Action(() => this.ScriptingPanelViewModel.OutputTerminal.AppendText(string.Format("\nAn error occured during the execution of the script !\nError: {0}\n", ex.Message))));
                return null;
            }
        }
        
        /// <summary>
        /// Get the engineeringmodel by name
        /// </summary>
        /// <param name="data">
        /// List of engineeringmodels
        /// </param>
        /// <param name="engineeringModelShortModelName">
        /// The name to search for
        /// </param>
        /// <returns>
        /// The found engineeringmodel
        /// null if not found
        /// </returns>
        internal EngineeringModel FindEngineeringModel(IEnumerable<EngineeringModel> data, string engineeringModelShortModelName)
        {
            foreach (var engineeringModel in data)
            {
                if (engineeringModel.EngineeringModelSetup.UserFriendlyShortName.ToLower()
                    == engineeringModelShortModelName.ToLower())
                {
                    return engineeringModel;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the iteration by number
        /// </summary>
        /// <param name="data">
        /// List of iterations
        /// </param>
        /// <param name="iterationNumber">
        /// The iteration number to search for
        /// </param>
        /// <returns>
        /// The found iteration
        /// null if not found
        /// </returns>
        internal Iteration FindIterationNumber(IEnumerable<Iteration> data, string iterationNumber)
        {
            foreach (var iteration in data)
            {
                if (iteration.IterationSetup.IterationNumber.ToString() == iterationNumber)
                {
                    return iteration;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the element by name
        /// </summary>
        /// <param name="data">
        /// List of elements
        /// </param>
        /// <param name="elementShortName">
        /// The element name to search for
        /// </param>
        /// <returns>
        /// The found element
        /// null if not found
        /// </returns>
        internal ElementDefinition FindElement(IEnumerable<ElementDefinition> data, string elementShortName)
        {
            foreach (var elementDefinition in data)
            {
                if (elementDefinition.UserFriendlyShortName.ToLower() == elementShortName.ToLower())
                {
                    return elementDefinition;
                } 
            }
       
            return null;
        }

        /// <summary>
        /// Get the parameter by name
        /// </summary>
        /// <param name="data">
        /// List of elements
        /// </param>
        /// <param name="parameterName">
        /// The parameter name to search for
        /// </param>
        /// <returns>
        /// The found parameter
        /// null if not found
        /// </returns>
        internal Parameter FindParameter(IEnumerable<Parameter> data, string parameterName)
        {
            foreach (var parameter in data)
            {
                if (parameter.UserFriendlyShortName.ToLower() == parameterName.ToLower())
                {
                    return parameter;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the value by type
        /// </summary>
        /// <param name="data">
        /// List of elements
        /// </param>
        /// <param name="parameterSwitch">
        /// The value and the type to search for
        /// </param>
        /// <returns>
        /// The found value
        /// null if not found
        /// </returns>
        internal string FindValue(IEnumerable<ParameterValueSet> data, string parameterSwitch)
        {
            foreach (var valueSet in data)
            {
                switch (parameterSwitch.ToLower())
                    {
                        case "actualvalue":
                            return valueSet.ActualValue.FirstOrDefault();

                        case "computed":    
                            return valueSet.Computed.FirstOrDefault();

                        case "formula":
                            return valueSet.Formula.FirstOrDefault();

                        case "manual":
                            return valueSet.Manual.FirstOrDefault();

                        case "published":
                            return valueSet.Published.FirstOrDefault();

                        // By default return reference value
                        default:
                            return valueSet.Reference.FirstOrDefault();    
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the <see cref="EngineeringModel"/>s from the <see cref="Assembler.Cache"/> and check if an EngineeringModel has 
        /// the same short name than the string entered in parameter.
        /// </summary>
        /// <param name="engineeringModelShortName"> 
        /// The short name for which we want to search if it matches with the short name of an <see cref="EngineeringModel"/>
        /// </param>
        /// <returns>
        /// The <see cref="EngineeringModel"/> which has the the same short name than the one entered in parameter.
        /// Null if no <see cref="EngineeringModel"/> has a short name that matches with the string entered in parameter.
        /// </returns>
        [Documentation("GetEngineeringModel(string engineeringModelShortName)",
        "Gets and engineering model.\nA connection to a data source and an open model are required")]
        public EngineeringModel GetEngineeringModel(string engineeringModelShortName)
        {
            if (this.ScriptingPanelViewModel == null)
            {
                return null;
            }

            if (this.ScriptingPanelViewModel.SelectedSession == null)
            {
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Input,
                    new Action(() => 
                        this.ScriptingPanelViewModel.OutputTerminal.AppendText("No session is selected to run the script. You need to be connected to execute this script.\n")));
                return null;
            }

            var engineeringModels = this.ScriptingPanelViewModel.SelectedSession.Assembler.Cache.Select(x => x.Value)
                       .Where(lazy => lazy.Value.ClassKind == ClassKind.EngineeringModel)
                       .Select(lazy => lazy.Value)
                       .Cast<EngineeringModel>();

            foreach (var engineeringModel in engineeringModels)
            {
                if (engineeringModel.EngineeringModelSetup.UserFriendlyShortName.ToLower()
                    == engineeringModelShortName.ToLower())
                {
                    return engineeringModel;
                }
            }

            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Input,
                new Action(() => 
                    this.ScriptingPanelViewModel.OutputTerminal.AppendText(string.Format("Engineering model {0} not found.\n", engineeringModelShortName))));
            return null;
        }

        /// <summary>
        /// Retrieves the <see cref="Iteration"/> of an <see cref="EngineeringModel"/> that matches with the parameter.
        /// </summary>
        /// <param name="engineeringModelShortName">
        /// The short name for which we want to search if it matches with the short name of an <see cref="EngineeringModel"/>.
        /// </param>
        /// <param name="iterationNumber">The number of the <see cref="Iteration"/> asked by the user.</param>
        /// <returns>
        /// The <see cref="Iteration"/> that matches with the information passed in parameters.
        /// Null if there is no match.
        /// </returns>
        [Documentation("GetEngineeringModelIteration(string engineeringModelShortName, int iterationNumber)",
        "Gets and engineering model iteration.\nA connection to a data source and an open model are required")]
        public Iteration GetEngineeringModelIteration(string engineeringModelShortName, int iterationNumber)
        {
            var engineeringModel = this.GetEngineeringModel(engineeringModelShortName);
            if (engineeringModel == null)
            {
                return null;
            }

            foreach (var iteration in engineeringModel.Iteration)
            {
                if (iteration.IterationSetup.IterationNumber == iterationNumber)
                {
                    return iteration;
                }
            }

            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Input,
                new Action(() => 
                    this.ScriptingPanelViewModel.OutputTerminal.AppendText(string.Format("Iteration {0} not found for the engineering model {1}\n", iterationNumber, engineeringModelShortName))));
            return null;
        }

        /// <summary>
        /// Retrieves the <see cref="ElementDefinition"/> that matches with the information passed in parameters.
        /// </summary>
        /// <param name="engineeringModelShortName">
        /// The short name for which we want to search if it matches with the short name of an <see cref="EngineeringModel"/>.
        /// </param>
        /// <param name="iterationNumber">
        /// The number for which we want to search if it matches with the number of an <see cref="Iteration"/> of an <see cref="EngineeringModel"/>.
        /// </param>
        /// <param name="elementDefinitionName">
        /// The name for which we want to search if it matches with the name of an <see cref="ElementDefinition"/> of an <see cref="Iteration"/>.
        /// </param>
        /// <returns>
        /// The <see cref="ElementDefinition"/> that matches with the information passed in parameters.
        /// Null if there is no match.
        /// </returns>
        [Documentation("GetElementDefinition(string engineeringModelShortName, int iterationNumber, string elementDefinitionName)",
        "Gets an element definition.\nA connection to a data source and an open model are required")]
        public ElementDefinition GetElementDefinition(string engineeringModelShortName, int iterationNumber, string elementDefinitionName)
        {
            var iteration = this.GetEngineeringModelIteration(engineeringModelShortName, iterationNumber);
            if (iteration == null)
            {
                return null;
            }

            foreach (var elementDefinition in iteration.Element)
            {
                if (elementDefinition.UserFriendlyShortName.ToLower() == elementDefinitionName.ToLower())
                {
                    return elementDefinition;
                }
            }

            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Input,
                new Action(() =>
                    this.ScriptingPanelViewModel.OutputTerminal.AppendText(string.Format("Element definition {0} for the iteration {1} of the engineering model {2} was not found.",
                    elementDefinitionName, iterationNumber, engineeringModelShortName))));
            return null;

        }

        /// <summary>
        /// Retrieves the <see cref="Parameter"/> that matches with the information passed in parameters.
        /// </summary>
        /// <param name="engineeringModelShortName">
        /// The short name for which we want to search if it matches with the short name of an <see cref="EngineeringModel"/>.
        /// </param>
        /// <param name="iterationNumber">
        /// The number for which we want to search if it matches with the number of an <see cref="Iteration"/> of an <see cref="EngineeringModel"/>.
        /// </param>
        /// <param name="elementDefinitionName">
        /// The name for which we want to search if it matches with the name of an <see cref="ElementDefinition"/> of an <see cref="Iteration"/>.
        /// </param>
        /// <param name="parameterName">
        /// The name for which we want to search if it matches with the name of a <see cref="Parameter"/> of an <see cref="ElementDefinition"/>.
        /// </param>
        /// <returns> 
        /// The <see cref="Parameter"/> that matches with the information passed in parameters.
        /// Null if there is no match.
        /// </returns>
        [Documentation("GetParameter(string engineeringModelShortName, int iterationNumber, string elementDefinitionName, string parameterName)",
        "Gets a parameter.\nA connection to a data source and an open model are required")]
        public Parameter GetParameter(string engineeringModelShortName, int iterationNumber, string elementDefinitionName, string parameterName)
        {
            var elementDefinition = this.GetElementDefinition(engineeringModelShortName, iterationNumber, elementDefinitionName);
            if (elementDefinition == null)
            {
                return null;
            }

            parameterName = elementDefinitionName + "." + parameterName;
            foreach (var parameter in elementDefinition.Parameter)
            {
                if (parameter.UserFriendlyShortName.ToLower() == parameterName.ToLower())
                {
                    return parameter;
                }
            }

            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Input,
                new Action(() =>
                    this.ScriptingPanelViewModel.OutputTerminal.AppendText(
                        string.Format("Parameter {0} of the element definition {1} for the iteration {2} of the engineering model {3} was not found.",
                        parameterName, elementDefinitionName, iterationNumber, engineeringModelShortName))));
            return null;
        }

        // TODO T2505 : Write code generator to write the methods that allow a user to retrieve data from the ECSS-E-TM-20-25 uml model.
        // TODO example of methods that could be autogenerated : 
        // TODO GetValue(EngineeringModel engineeringModel, int iterationNumber, string elementDefinitionName, string parameterName, string parameterSwitch)
        // TODO GetValue(Iteration iteration, string elementDefinitionName, string parameterName, string parameterSwitch)
        // TODO GetValue(ElementDefinition elementDefinition, string parameterName, string parameterSwitch)
        // TODO GetValue(Parameter parameter, string parameterSwitch) => already created with the method FindValue(IEnumerable<ParameterValueSet> data, string parameterSwitch)
        // TODO And same logic for GetParameter, GetElementDefinition, etc..

        /// <summary>
        /// Retrieves the value that matches with the information passed in parameters.
        /// </summary>
        /// <param name="engineeringModelShortName">
        /// The short name for which we want to search if it matches with the short name of an <see cref="EngineeringModel"/>.
        /// </param>
        /// <param name="iterationNumber">
        /// The number for which we want to search if it matches with the number of an <see cref="Iteration"/> of an <see cref="EngineeringModel"/>.
        /// </param>
        /// <param name="elementDefinitionName">
        /// The name for which we want to search if it matches with the name of an <see cref="ElementDefinition"/> of an <see cref="Iteration"/>.
        /// </param>
        /// <param name="parameterName">
        /// The name for which we want to search if it matches with the name of a <see cref="Parameter"/> of an <see cref="ElementDefinition"/>.
        /// </param>
        /// <param name="parameterSwitch">
        /// The type of the value the user is looking for.
        /// </param>
        /// <returns> 
        /// The value that matches with the information passed in parameters.
        /// Null if there is no match.
        /// </returns>
        [Documentation("GetValue(string \"Name of the model (LOFT),Iteration number (1),Element name (FEEs),paramter name .property (mass_margin),value by field(published)\")",
            "Gets the engineering model data and return a value from an engineering model.\nA connection to a data source and an open model are required")]
        public string GetValue(string engineeringModelShortName, int iterationNumber, string elementDefinitionName, string parameterName, string parameterSwitch)
        {
            var parameter = this.GetParameter(engineeringModelShortName, iterationNumber, elementDefinitionName, parameterName);
            if (parameter == null)
            {
                return null;
            }

            foreach (var valueSet in parameter.ValueSet)
            {
                switch (parameterSwitch.ToLower())
                {
                    case "actualvalue":
                        return valueSet.ActualValue.FirstOrDefault();

                    case "computed":
                        return valueSet.Computed.FirstOrDefault();

                    case "formula":
                        return valueSet.Formula.FirstOrDefault();

                    case "manual":
                        return valueSet.Manual.FirstOrDefault();

                    case "published":
                        return valueSet.Published.FirstOrDefault();

                    // By default return reference value
                    default:
                        return valueSet.Reference.FirstOrDefault();
                }
            }

            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Input,
                new Action(() =>
                    this.ScriptingPanelViewModel.OutputTerminal.AppendText(
                        string.Format("Value of type {0} for Parameter {1} of the element definition {2} for the iteration {3} of the engineering model {4} was not found.",
                        parameterSwitch, parameterName, elementDefinitionName, iterationNumber, engineeringModelShortName))));
            return null;
        }

        /// <summary>
        /// Retrieves the <see cref="SiteDirectory"/> associated to the current <see cref="ISession"/>.
        /// </summary>
        /// <returns>The <see cref="SiteDirectory"/> associated to the current <see cref="ISession"/>.
        /// </returns>
        [Documentation("GetSiteDirectory()", "Gets the site directory associated to the selected session.")]
        public SiteDirectory GetSiteDirectory()
        {
            if (this.ScriptingPanelViewModel.SelectedSession == null)
            {
                Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Input,
                new Action(() =>
                    this.ScriptingPanelViewModel.OutputTerminal.AppendText("No session is selected to run the script. You need to be connected to execute this script.\n")));
                return null;
            }

            return this.ScriptingPanelViewModel.SelectedSession.Assembler.RetrieveSiteDirectory();
        }
        #endregion

        /// <summary>
        /// Initializes the list of the command completion data from the methods and properties decorated with an attribute.
        /// </summary>
        public void InitCommandCompletionData()
        {
            this.CommandCompletionData = new List<ICompletionData>();

            var methods = typeof(ScriptingProxy).GetMethods();

            var properties = typeof(ScriptingProxy).GetProperties();

            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes(typeof(DocumentationAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                foreach (var attr in attrs)
                {
                    var cmdAttr = attr as DocumentationAttribute;
                    if (cmdAttr?.Name != null && cmdAttr.Description != null)
                    {
                        this.CommandCompletionData.Add(new EditorCompletionData(cmdAttr.Name, cmdAttr.Description));
                    }
                }
            }

            foreach (var property in properties)
            {
                var attrs = property.GetCustomAttributes(typeof(DocumentationAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }

                foreach (var attr in attrs)
                {
                    var cmdAttr = attr as DocumentationAttribute;
                    if (cmdAttr?.Name != null && cmdAttr.Description != null)
                    {
                        this.CommandCompletionData.Add(new EditorCompletionData(cmdAttr.Name, cmdAttr.Description));
                    }
                }
            }
        }
    }
}
