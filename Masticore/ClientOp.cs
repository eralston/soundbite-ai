namespace Masticore
{
    /// <summary>
    /// Represents a request sent to the client to fulfil an operation that cannot be completed from
    /// the backend.  Normally these operations require user tokens from third-party providers.
    /// </summary>
    public class ClientOp
    {
        #region Properties

        /// <summary>
        /// Stores the name of the operation that needs to be executed on the client.
        /// </summary>        
        public string OpType { get; set; }

        /// <summary>
        /// Gets a data object with information needed by the client to execute the operation.
        /// </summary>        
        public object Data { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientOp"/> class.
        /// </summary>        
        /// <param name="opType">Name of the operation that needs to be executed by the client.</param>
        /// <param name="data">Data required by the client to execute the operation.</param>
        public ClientOp(string opType, object data)
        {
            OpType = opType;
            Data = data;
        }

        #endregion
    }
}