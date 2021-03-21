namespace WebAssembly.Browser.DOM
{
    /// <summary>
    /// Specifies how strings containing \n are to be written out.
    /// </summary>
    
    public enum Endings
    {
        /// <summary>
        /// Endings unchanged
        /// </summary>
        Transparent,

        /// <summary>
        /// Endings changed to match host OS filesystem convention
        /// </summary>
        Native
    }
}