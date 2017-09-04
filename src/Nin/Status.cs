namespace Types
{
    public enum Status
    {
        /// <summary>
        /// Currently active
        /// </summary>
        Gjeldende = 1,

        /// <summary>
        /// Superceded by a newer upload
        /// </summary>
        Utgått = 2,

        /// <summary>
        /// Stored in archive, not active
        /// </summary>
        Importert = 3,
    }
}
