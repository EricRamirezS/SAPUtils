namespace SAPUtils.Internal.Attributes.UserTables
{
    /// <summary>
    /// Represents a strongly-typed version of <see cref="IUserTableField"/>, enforcing type safety 
    /// for the default value and parsing methods.
    ///
    /// This interface provides a generic way to define user-defined fields (UDFs) in SAP Business One, 
    /// ensuring that values are correctly interpreted according to the expected type.
    ///
    /// **Usage:**  
    /// - Use this interface when you want to work with a specific data type (`T`) for the field.
    /// - The `DefaultValue` and `ParseValue` methods are overridden to return `T` instead of `object`.
    ///
    /// **See Also:** <see cref="IUserTableField"/>
    /// </summary>
    /// <typeparam name="T">The expected data type of the field.</typeparam>
    internal interface IUserTableField<out T> : IUserTableField
    {
        /// <summary>
        /// Gets the default value of the user-defined field, strongly typed as `T`.
        /// </summary>
        new T DefaultValue { get; }

        /// <summary>
        /// Parses a given object value into the expected type `T`.
        /// </summary>
        /// <param name="value">The raw value to be parsed.</param>
        /// <returns>The parsed value of type `T`.</returns>
        new T ParseValue(object value);
    }
}