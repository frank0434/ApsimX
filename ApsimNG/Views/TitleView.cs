﻿using System;
using Gtk;

namespace UserInterface.Views
{
    public delegate void TitleChangedDelegate(string NewText);

    /// <summary>
    /// Describes an interface for an axis view.
    /// </summary>
    interface ITitleView
    {
        event TitleChangedDelegate OnTitleChanged;

        void Populate(string title);
    }

    /// <summary>
    /// A Gtk# implementation of an TitleView
    /// </summary>
    public class TitleView : ViewBase, ITitleView
    {
        private string OriginalText;

        public event TitleChangedDelegate OnTitleChanged;

        private HBox hbox1 = null;
        private Entry entry1 = null;

        /// <summary>
        /// Construtor
        /// </summary>
        public TitleView(ViewBase owner) : base(owner)
        {
            Builder builder = MasterView.BuilderFromResource("ApsimNG.Resources.Glade.TitleView.glade");
            hbox1 = (HBox)builder.GetObject("hbox1");
            entry1 = (Entry)builder.GetObject("entry1");
            _mainWidget = hbox1;
            entry1.Changed += OnPositionComboChanged;
            _mainWidget.Destroyed += _mainWidget_Destroyed;
        }

        private void _mainWidget_Destroyed(object sender, EventArgs e)
        {
            entry1.Changed -= OnPositionComboChanged;
            _mainWidget.Destroyed -= _mainWidget_Destroyed;
            _owner = null;
        }

        /// <summary>
        /// Populate the view with the specified title.
        /// </summary>
        public void Populate(string title)
        {
            entry1.Text = title;
        }

        /// <summary>
        /// When the user 'enters' the position combo box, save the current text value for later.
        /// </summary>
        private void OnTitleTextBoxEnter(object sender, EventArgs e)
        {
            OriginalText = entry1.Text;
        }

        /// <summary>
        /// When the user changes the combo box check to see if the text has changed. 
        /// If so then invoke the 'OnPositionChanged' event so that the presenter can pick it up.
        /// </summary>
        private void OnPositionComboChanged(object sender, EventArgs e)
        {
            if (OriginalText == null)
                OriginalText = entry1.Text;
            if (entry1.Text != OriginalText && OnTitleChanged != null)
            {
                OriginalText = entry1.Text;
                OnTitleChanged.Invoke(entry1.Text);
            }
        }
    }
}
