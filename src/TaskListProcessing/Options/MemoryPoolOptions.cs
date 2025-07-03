namespace TaskListProcessing.Options
{

    /// <summary>
    /// Memory pool configuration options.
    /// </summary>
    public class MemoryPoolOptions
    {
        /// <summary>
        /// Gets or sets the initial pool size.
        /// </summary>
        public int InitialPoolSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the maximum pool size.
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets the pool growth factor when expanding.
        /// </summary>
        public double GrowthFactor { get; set; } = 1.5;

        /// <summary>
        /// Gets or sets the pool shrink threshold (percentage of unused items).
        /// </summary>
        public double ShrinkThreshold { get; set; } = 0.7;

        /// <summary>
        /// Gets or sets whether to pre-warm the pool on initialization.
        /// </summary>
        public bool PrewarmPool { get; set; } = true;

        /// <summary>
        /// Validates the memory pool options.
        /// </summary>
        /// <returns>Validation result.</returns>
        public (bool IsValid, string[] Errors) Validate()
        {
            var errors = new List<string>();

            if (InitialPoolSize <= 0)
                errors.Add("InitialPoolSize must be greater than 0");

            if (MaxPoolSize <= 0)
                errors.Add("MaxPoolSize must be greater than 0");

            if (MaxPoolSize < InitialPoolSize)
                errors.Add("MaxPoolSize cannot be less than InitialPoolSize");

            if (GrowthFactor <= 1.0)
                errors.Add("GrowthFactor must be greater than 1.0");

            if (ShrinkThreshold <= 0.0 || ShrinkThreshold >= 1.0)
                errors.Add("ShrinkThreshold must be between 0.0 and 1.0");

            return (errors.Count == 0, errors.ToArray());
        }
    }
}
