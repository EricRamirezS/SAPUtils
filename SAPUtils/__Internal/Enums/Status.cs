using System;
using SAPUtils.Forms;

namespace SAPUtils.__Internal.Enums {
    /// <summary>
    /// Describes the state or lifecycle stage of an entity, used for tracking changes
    /// or modifications in data operations.
    /// </summary>
    /// <remarks>
    /// The <see cref="Status"/> enumeration provides various states that represent the changes
    /// applied to an entity. Each state conveys the entity's relation to its original, current,
    /// or future state during data processing.
    /// </remarks>
    /// <seealso cref="SAPUtils.__Internal.Enums.Status.Normal"/>
    /// <seealso cref="SAPUtils.__Internal.Enums.Status.Modified"/>
    /// <seealso cref="SAPUtils.__Internal.Enums.Status.ModifiedRestored"/>
    /// <seealso cref="SAPUtils.__Internal.Enums.Status.New"/>
    /// <seealso cref="SAPUtils.__Internal.Enums.Status.Discard"/>
    /// <seealso cref="SAPUtils.__Internal.Enums.Status.Delete"/>
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
        /// <seealso cref="Discard"/>
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

        /// <summary>
        /// The object was previously soft-deleted and is now marked for restoration.
        /// Additionally, one or more of its fields have been modified since deletion.
        /// </summary>
        /// <seealso cref="SAPUtils.__Internal.Enums.Status.Modified"/>
        /// <seealso cref="SAPUtils.__Internal.Enums.Status.Discard"/>
        /// <seealso cref="SAPUtils.__Internal.Enums.Status.Delete"/>
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
        /// <seealso cref="Discard"/>
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
        Discard,

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

    internal static class StatusExtensions {
        public static string GetReadableName(this Status status) {
            switch (status) {
                case Status.Normal:
                    return "✅ Normal";
                case Status.Modified:
                    return "✏️ Modificar";
                case Status.ModifiedRestored:
                    return "🔁 Restaurar";
                case Status.New:
                    return "➕ Nuevo";
                case Status.Discard:
                    return "❌ Descartar";
                case Status.Delete:
                    return "🗑️ Eliminar";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}