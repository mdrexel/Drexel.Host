namespace Drexel.Host
{
    /// <summary>
    /// Exit codes for the application.
    /// </summary>
    public static class ExitCode
    {
        /// <summary>
        /// Indicates the application completed successfully.
        /// </summary>
        public const byte Success = 0;

        /// <summary>
        /// Indicates that the application failed for an unspecified reason.
        /// </summary>
        public const byte UnspecifiedFailure = 1;

        /// <summary>
        /// Indicates that the application failed because it was incorrectly invoked.
        /// </summary>
        public const byte IncorrectInvocation = 2;
    }
}
