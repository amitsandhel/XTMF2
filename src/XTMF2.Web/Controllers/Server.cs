namespace XTMF2.Web.Controllers
{
    /// <summary>
    /// Provides a controller for accessing XTMF2
    /// </summary>
    public static class Server
    {
        /// <summary>
        /// The singleston instance of the runtime that will be used.
        /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static XTMFRuntime Runtime { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        internal static void InitializeXTMF()
        {
            Runtime = XTMFRuntime.CreateRuntime();
        }

        internal static void ShutdownXTMF()
        {
            Runtime.Shutdown();
            Runtime = null!;
        }
    }
}
