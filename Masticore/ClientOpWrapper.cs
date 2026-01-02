using System.Linq;

namespace Masticore
{
    /// <summary>
    /// Some server-side operations require fulfillment of client-side actions to interact with 
    /// third-party service providers.  This wrapper is intended to wrap a result along with a list
    /// of additional actions that need to occur client-side.
    /// </summary>
    /// <typeparam name="TResult">.NET type of the result to wrap.</typeparam>
    public class ClientOpWrapper<TResult>
    {
        #region Properties

        /// <summary>
        /// Stores the data that is being wrapped along with client-side operations.
        /// </summary>        
        public TResult Result { get; set; }

        /// <summary>
        /// List of items representing client-side operations that need to occur.  
        /// </summary>        
        public ClientOp[] ClientOps { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientOpWrapper{T}"/> class.
        /// </summary>
        public ClientOpWrapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientOpWrapper{T}"/> class.
        /// </summary>
        /// <param name="data">Data to wrap.</param>
        /// <param name="clientOps">Client operations that need to be completed.</param>
        public ClientOpWrapper(TResult data, params ClientOp[] clientOps)
        {
            Result = data;
            ClientOps = clientOps?.ToArray();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds one or more client operations to the existing list of operations.  Order is maintained.
        /// </summary>
        /// <param name="clientOps">Client ops to addd to existing client ops.</param>
        public void AddClientOps(params ClientOp[] clientOps)
        {
            if (clientOps != null && clientOps.Length > 0)
            {
                if (ClientOps == null)
                {
                    ClientOps = clientOps;
                }
                else
                {
                    System.Collections.Generic.List<ClientOp> temp = ClientOps.ToList();
                    temp.AddRange(clientOps);
                    ClientOps = temp.ToArray();
                }
            }
        }

        #endregion
    }
}