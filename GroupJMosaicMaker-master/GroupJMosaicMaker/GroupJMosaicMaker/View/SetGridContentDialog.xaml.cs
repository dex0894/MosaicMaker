using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace GroupJMosaicMaker.View
{
    /// <summary>
    /// Prompts the user to set the grid value.
    /// </summary>
    /// <seealso cref="Windows.UI.Xaml.Controls.ContentDialog" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector" />
    /// <seealso cref="Windows.UI.Xaml.Markup.IComponentConnector2" />
    public sealed partial class SetGridDialog : ContentDialog
    {

        /// <summary>
        ///     User input from text box
        /// </summary>
        public string UserText { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetGridDialog"/> class.
        /// </summary>
        public SetGridDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.UserText = this.userInput.Text;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void UserInput_TextChanged(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (Regex.IsMatch(sender.Text, "^\\d{0,2}$"))
            {
                return;
            }

            var pos = sender.SelectionStart - 1;
            sender.Text = sender.Text.Remove(pos, 1);
            sender.SelectionStart = pos;
        }
    }
}
