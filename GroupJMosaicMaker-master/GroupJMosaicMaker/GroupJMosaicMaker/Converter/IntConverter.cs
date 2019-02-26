using System;
using Windows.UI.Xaml.Data;

namespace GroupJMosaicMaker.Converter
{
    /// <inheritdoc />
    /// <summary>
    ///     Helps with binding int to string text boxes
    /// </summary>
    /// <seealso cref="T:Windows.UI.Xaml.Data.IValueConverter" />
    public class IntConverter : IValueConverter
    {
        #region Methods

        /// <summary>
        ///     Converts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value?.ToString();
        }

        /// <summary>
        ///     Converts the back.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="parameter">The parameter.</param>
        /// <param name="language">The language.</param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var isNumeric = int.TryParse(value.ToString(), out var n);
            return isNumeric ? n : 0;
        }

        #endregion
    }
}