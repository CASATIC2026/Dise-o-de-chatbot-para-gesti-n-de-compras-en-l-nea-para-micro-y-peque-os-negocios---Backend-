namespace Services.Inventario.Validators
{
    /// <summary>
    /// Represents a standardized container for paged data results.
    /// </summary>
    /// <typeparam name="T">The type of the items contained in the result set.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Gets or sets the list of items for the current page.
        /// </summary>
        public List<T> Items { get; set; } = new();

        /// <summary>
        /// Gets or sets the total count of items available in the data source, 
        /// used to calculate total pages on the client side.
        /// </summary>
        public int TotalCount { get; set; }
    }
}