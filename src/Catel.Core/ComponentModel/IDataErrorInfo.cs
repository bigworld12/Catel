﻿#if UAP_DEFAULT

namespace System.ComponentModel
{
    /// <summary>
    /// WinRT implementation of the <c>IDataErrorInfo</c> interface.
    /// </summary>
    public interface IDataErrorInfo
    {
        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>
        /// An error message indicating what is possibly wrong with this object. The default is
        /// an empty string ("").
        /// </value>
        string Error { get; }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <value></value>
        /// <param name="columnName">The name of the property whose error message to get.</param>
        string this[string columnName] { get; }
    }
}

#endif
