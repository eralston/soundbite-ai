namespace Soundbite.Services
{
    /// <summary>
    /// A presentation of the potentially unique consumption-pricing for a unique org, plus calculation of estimated pricing for a time period, each field of which is a complete display value with number and relevant units
    /// </summary>
    /// <remarks>
    /// Keep in mind:
    /// 1) this is NOT a real invoice, only an estimate
    /// 2) Each organization may have its own prices and scalers based on their unique discounts, etc
    /// </remarks>
    public class PricingProjectionReport : ReportBase
    {
        /// <summary>
        /// An official roll-up of other line items that offers an estimate (but not a real invoice) of costs during a time period
        /// </summary>
        public string TotalPrice { get; set; }

        /// <summary>
        /// Any additional taxes and fees based on territory that will be assessed for the time period
        /// </summary>
        public string TaxesAndFees { get; set; }

        /// <summary>
        /// The monthly base cost for this organization
        /// </summary>
        public string BasePrice { get; set; }

        /// <summary>
        /// The total consumption cost for content produced
        /// </summary>
        public string ProducerPrice { get; set; }

        /// <summary>
        /// The unit price per unit of content produced
        /// </summary>
        public string ProducerUnitPrice { get; set; }

        /// <summary>
        /// The total consumption cost for content consumed
        /// </summary>
        public string ConsumerPrice { get; set; }

        /// <summary>
        /// The total consumption cost per unit of content consumed
        /// </summary>
        public string ConsumerUnitPrice { get; set; }
    }
}
