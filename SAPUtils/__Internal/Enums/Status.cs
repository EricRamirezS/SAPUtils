using SAPUtils.Forms;

namespace SAPUtils.__Internal.Enums {
    /// <summary>
    /// Represents a newly created item or record within the context of the application.
    /// </summary>
    /// <remarks>
    /// This status is used to indicate that the item has been newly added but not yet persisted or finalized.
    /// </remarks>
    /// <seealso cref="SAPUtils.Forms.ChangeTrackerMatrixForm{T}" />
    /// <seealso cref="Status.Normal" />
    /// <seealso cref="Status.Modified" />
    /// <seealso cref="Status.NewDelete" />
    /// <seealso cref="Status.Delete" />
    internal enum Status {
        /// <summary>
        /// Represents the default or unmodified state of an entity.
        /// An entity with the <see cref="Normal"/> status is considered unchanged
        /// compared to its stored or original state.
        /// </summary>
        /// <remarks>
        /// This status is typically assigned to items that do not require updates, deletions,
        /// or other modifications during data processing or tracking.
        /// </remarks>
        /// <seealso cref="SAPUtils.__Internal.Enums.Status.Modified"/>
        /// <seealso cref="SAPUtils.__Internal.Enums.Status.New"/>
        /// <seealso cref="SAPUtils.__Internal.Enums.Status.Delete"/>
        /// <seealso cref="SAPUtils.__Internal.Enums.Status.NewDelete"/>
        Normal,

        /// <summary>
        /// Represents a state indicating that an item has been modified but not yet saved or finalized.
        /// </summary>
        /// <remarks>
        /// This state is used in scenarios where changes to data in a particular
        /// context need to be tracked without immediately committing those changes.
        /// </remarks>
        /// <seealso cref="SAPUtils.Forms.ChangeTrackerMatrixForm{T}"/>
        Modified,

        ModifiedRestored,

        /// <summary>
        /// Represents a newly created item or record within the context of the application.
        /// </summary>
        /// <remarks>
        /// This status is used to indicate that the item has been newly added
        /// but not yet persisted or finalized.
        /// </remarks>
        /// <seealso cref="SAPUtils.Forms.ChangeTrackerMatrixForm{T}"/>
        /// <seealso cref="Status.Normal"/>
        /// <seealso cref="Status.Modified"/>
        /// <seealso cref="Status.NewDelete"/>
        /// <seealso cref="Status.Delete"/>
        New,

        /// <summary>
        /// Represents an entity that has been both newly created and subsequently marked for deletion
        /// within the same processing cycle.
        /// </summary>
        /// <remarks>
        /// This status is assigned to items that are added and then removed before being persisted or finalized.
        /// Such entities exist temporarily and are subject to cancellation or removal without further processing.
        /// </remarks>
        /// <seealso cref="SAPUtils.__Internal.Enums.Status.New"/>
        /// <seealso cref="SAPUtils.__Internal.Enums.Status.Delete"/>
        /// <seealso cref="SAPUtils.__Internal.Enums.Status.Normal"/>
        /// <seealso cref="SAPUtils.__Internal.Enums.Status.Modified"/>
        NewDelete,

        /// <summary>
        /// Represents a deletion state within the <see cref="Status"/> enumeration.
        /// </summary>
        /// <remarks>
        /// This state is used for marking items as deleted in a change-tracking context.
        /// The <see cref="Delete"/> status is commonly utilized in conjunction with user interfaces
        /// or data models to flag entries intended for removal, potentially relying on "soft-delete"
        /// functionality.
        /// </remarks>
        /// <seealso cref="ChangeTrackerMatrixForm{T}"/>
        Delete,
    }
}