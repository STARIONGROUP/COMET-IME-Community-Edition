// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IScriptingProxy.cs" company="RHEA System S.A.">
//   Copyright (c) 2017 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Scripting.Interfaces
{
    using System.Collections.Generic;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.Composition;
    using ICSharpCode.AvalonEdit.CodeCompletion;

    /// <summary>
    /// represents the interface for the <see cref="Helpers.ScriptingProxy"/> class;
    /// </summary>
    public interface IScriptingProxy
    {
        /// <summary>
        /// The view-model which calls the method of this interface. 
        /// </summary>
        IScriptPanelViewModel ScriptingPanelViewModel { get; set; }

        /// <summary>
        /// The <see cref="CDP4Common.Helpers.NestedElementTreeGenerator"/> object that the user can use. 
        /// </summary>
        NestedElementTreeGenerator NestedElementTreeGenerator { get; }

        /// <summary>
        /// Gets or sets the list of the command completion data that are used to suggest properties and methods to the user.
        /// </summary>
        IList<ICompletionData> CommandCompletionData { get; }

        #region CDP4Commands

        /// <summary>
        /// A command to clear the output.
        /// </summary>
        void Clear();

        /// <summary>
        /// Displays all the commands possible and explains how to use it. 
        /// </summary>
        void Help();

        /// <summary>
        /// Opens the dialog associated to the <see cref="INameMetaData.Name"/> if the <see cref="dialogName"/> matches.
        /// </summary>
        /// <param name="dialogName">The dialog name entered by the user in the script.</param>
        void OpenDialog(string dialogName);

        /// <summary>
        /// Opens the model browser if a session is active. 
        /// </summary>
        void OpenPanelModelBrowser();

        #endregion
        #region DataCommands

        /// <summary>
        /// Clears the variables of the scope of the script.
        /// </summary>
        void ClearVariables();

        /// <summary>
        /// Gets the engineering model data 
        /// </summary>
        /// <param name="texteditorCommand">The string to be given to search through the session.</param>
        /// <returns>A string with a value from an engineering model.</returns>
        string ModelCode(string texteditorCommand);

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
        EngineeringModel GetEngineeringModel(string engineeringModelShortName);

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
        Iteration GetEngineeringModelIteration(string engineeringModelShortName, int iterationNumber);

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
        ElementDefinition GetElementDefinition(string engineeringModelShortName, int iterationNumber, string elementDefinitionName);

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
        Parameter GetParameter(string engineeringModelShortName, int iterationNumber, string elementDefinitionName, string parameterName);

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
        string GetValue(string engineeringModelShortName, int iterationNumber, string elementDefinitionName, string parameterName, string parameterSwitch);

        /// <summary>
        /// Retrieves the <see cref="SiteDirectory"/> associated to the current <see cref="ISession"/>.
        /// </summary>
        /// <returns>The <see cref="SiteDirectory"/> associated to the current <see cref="ISession"/>.
        /// </returns>
        SiteDirectory GetSiteDirectory();
        #endregion
    }
}
