namespace Brute_Force.Model
{
    /// <summary>
    /// The class representing an order.
    /// </summary>
    public class Order
    {
        #region Fields

        private int productId; // id of the product
        private int orderTime; // time of the order

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the id of the product.
        /// </summary>
        public int ProductId { get { return productId; } set { productId = value; } }

        /// <summary>
        /// Get or sets the time of the order.
        /// </summary>
        public int OrderTime { get { return orderTime; } set { orderTime = value; } }

        #endregion
    }
}
