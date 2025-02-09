namespace Drexel.Host.Commands.Power
{
    /// <summary>
    /// Represents the reason for the power-off operation.
    /// </summary>
    public enum PowerOffReason
    {
        /// <summary>
        /// Indicates that a reason was not provided.
        /// </summary>
        None,

        /// <summary>
        /// Indicates that the reason is a power failure.
        /// </summary>
        Power,

        /// <summary>
        /// Indicates that the reason is a software failure.
        /// </summary>
        Software,

        /// <summary>
        /// Indicates that the reason is a hardware failure.
        /// </summary>
        Hardware,
    }
}
