// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContextMenuItemViewModel.cs" company="RHEA System S.A.">
//   Copyright (c) 2015 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4Composition.Mvvm
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using CDP4Common.CommonData;
    using ReactiveUI;

    /// <summary>
    /// The view-model for context menus
    /// </summary>
    public class ContextMenuItemViewModel : ReactiveObject
    {
        /// <summary>
        /// Backing field for <see cref="Header"/>
        /// </summary>
        private string header;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenuItemViewModel"/> class
        /// </summary>
        /// <param name="header">The header for this menu item</param>
        /// <param name="inputGestureText">The Input Gesture text can shows a keyboard combination</param>
        /// <param name="command">The <see cref="ICommand"/> bound to this command</param>
        /// <param name="menuItemKind">The <see cref="MenuItemKind"/> of this <see cref="ContextMenuItemViewModel"/></param>
        /// <param name="thingKind">The <see cref="ClassKind"/> this menu-item operates on</param>
        public ContextMenuItemViewModel(string header, string inputGestureText, ICommand command, MenuItemKind menuItemKind = MenuItemKind.None, ClassKind thingKind = ClassKind.Thing)
        {
            this.Header = header;
            this.InputGestureText = inputGestureText;
            this.MenuCommand = command;
            this.MenuItemKind = menuItemKind;
            this.thingKind = thingKind;
            this.SubMenu = new List<ContextMenuItemViewModel>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenuItemViewModel"/> class
        /// </summary>
        /// <param name="header">The header for this menu item</param>
        /// <param name="inputGestureText">The Input Gesture text can shows a keyboard combination</param>
        /// <param name="executeCommandAction">The action that the <see cref="ReactiveCommand{Object}"/> shall execute</param>
        /// <param name="canExecute">The state of the command</param>
        /// <param name="menuItemKind">The <see cref="MenuItemKind"/></param>
        /// <param name="thingKind">The <see cref="ClassKind"/> this menu-item operates on</param>
        public ContextMenuItemViewModel(string header, string inputGestureText, Action<Thing> executeCommandAction, bool canExecute, MenuItemKind menuItemKind = MenuItemKind.None, ClassKind thingKind = ClassKind.Thing)
        {
            this.CanExecute = canExecute;
            this.MenuCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanExecute));
            ((ReactiveCommand<object>)this.MenuCommand).Subscribe(_ => executeCommandAction(this.RelatedThing));
            this.Header = header;
            this.InputGestureText = inputGestureText;
            this.MenuItemKind = menuItemKind;
            this.thingKind = thingKind;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenuItemViewModel"/> class
        /// </summary>
        /// <param name="header">The header for this menu item</param>
        /// <param name="inputGestureText">The Input Gesture text can shows a keyboard combination</param>
        /// <param name="executeCommandAction">The action that the <see cref="ReactiveCommand{Object}"/> shall execute</param>
        /// <param name="thing">The <see cref="Thing"/> related to the command</param>
        /// <param name="canExecute">The state of the command</param>
        /// <param name="menuItemKind">The <see cref="MenuItemKind"/></param>
        public ContextMenuItemViewModel(string header, string inputGestureText, Action<Thing> executeCommandAction, Thing thing, bool canExecute, MenuItemKind menuItemKind = MenuItemKind.None)
        {
            this.CanExecute = canExecute;
            this.MenuCommand = ReactiveCommand.Create(this.WhenAnyValue(x => x.CanExecute));
            ((ReactiveCommand<object>)this.MenuCommand).Subscribe(_ => executeCommandAction(this.RelatedThing));
            this.Header = header;
            this.InputGestureText = inputGestureText;
            this.RelatedThing = thing;
            this.MenuItemKind = menuItemKind;
            this.thingKind = thing.ClassKind;
        }

        /// <summary>
        /// Gets or sets the menu text
        /// </summary>
        public string Header
        {
            get { return this.header; }
            set { this.RaiseAndSetIfChanged(ref this.header, value); }
        }

        /// <summary>
        /// Gets the related <see cref="Thing"/> for this <see cref="ContextMenuItemViewModel"/> if any
        /// </summary>
        public Thing RelatedThing { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="MenuCommand"/> can be executed
        /// </summary>
        public bool CanExecute { get; private set; }

        /// <summary>
        /// Gets this <see cref="ContextMenuItemViewModel"/>'s <see cref="ICommand"/>
        /// </summary>
        public ICommand MenuCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="MenuItemKind"/>
        /// </summary>
        public MenuItemKind MenuItemKind { get; private set; }

        /// <summary>
        /// Gets the <see cref="ClassKind"/> this menu-item operates on
        /// </summary>
        public ClassKind thingKind { get; private set; }

        /// <summary>
        /// Gets the sub-menu of this <see cref="ContextMenuItemViewModel"/>
        /// </summary>
        public List<ContextMenuItemViewModel> SubMenu { get; private set; }

        /// <summary>
        /// Gets the <see cref="InputGestureText"/> that is used to display on the context menu
        /// </summary>
        /// <remarks>
        /// The <see cref="InputGestureText"/> is not bound to a command from the context menu item, this is 
        /// achieved through the Window.InputBindings
        /// </remarks>
        public string InputGestureText { get; private set; }
    }
}