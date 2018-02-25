using System;
using System.Collections.Generic;

namespace Flogging.Core
{
    public class FlogDetail
    {
        public FlogDetail()
        {
            Timestamp = DateTime.Now;
        }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        #region WHERE
        public string Product { get; set; }
        public string Layer { get; set; }
        public string Location { get; set; }
        public string Hostname { get; set; }
        #endregion
        #region WHO
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        #endregion
        #region EVERYTHING ELSE
        /// <summary>
        /// only for performance entries
        /// </summary>
        public long? ElapsedMilliseconds { get; set; }
        /// <summary>
        /// the exception for error logging
        /// </summary>
        public Exception Exception { get; set; }
        public CustomException CustomException { get; set; }
        /// <summary>
        /// exception shielding from server to client
        /// </summary>
        public string CorrelationId { get; set; }
        /// <summary>
        /// catch-all for anything else
        /// </summary>
        public Dictionary<string, object> AdditionalInfo { get; set; }
        #endregion
    }
}
