using System;
using System.Text;

namespace PatioFIX.Common.FixSupport
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class SessionId : IEquatable<SessionId>
    {
        /// <summary>
        /// SenderCompID (Tag = 49, Type: String)
        /// Assigned value used to identify firm sending message.
        /// </summary>
        public string SenderCompID { get; }
        /// <summary>
        /// SenderSubID (Tag = 50, Type: String)
        /// Assigned value used to identify specific message originator (desk, trader, etc.)
        /// </summary>
        public string SenderSubID { get; }

        /// <summary>
        /// TargetCompID (Tag = 56, Type: String)
        /// Assigned value used to identify receiving firm.
        /// </summary>
        public string TargetCompID { get; }
        /// <summary>
        /// TargetSubID (Tag = 57, Type: String)
        /// Assigned value used to identify specific individual or unit intended to receive message. 
        /// </summary>
        public string TargetSubID { get; }

        /// <summary>
        /// Returns CustomKey - additional id to differentiate sessions with the same (senderCompId, 
        /// targetCompId, fixVersion); null means no token required
        /// </summary>
        public string CustomKey { get; }


        /// <summary>
        /// Returns session id string key Key format is SenderCompId-TargetCompId-FixVersion-CustomKey 
        /// where "-CustomKey" part is omitted if token length is 0.
        /// </summary>
        public string Id { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="senderCompID"></param>
        /// <param name="targetCompID"></param>
        /// <param name="customKey"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SessionId(string senderCompID, string targetCompID, string customKey = null)
        {
            if (String.IsNullOrWhiteSpace(senderCompID))
                throw new ArgumentNullException(nameof(senderCompID));
            if (String.IsNullOrWhiteSpace(targetCompID))
                throw new ArgumentNullException(nameof(targetCompID));


            this.SenderCompID = senderCompID.Trim().ToUpperInvariant();
            this.TargetCompID = targetCompID.Trim().ToUpperInvariant();
            if (!String.IsNullOrWhiteSpace(customKey))
                this.CustomKey = customKey.ToUpperInvariant();

            MakeID();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="senderCompID"></param>
        /// <param name="senderSubID"></param>
        /// <param name="targetCompID"></param>
        /// <param name="targetSubID"></param>
        /// <param name="customKey"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SessionId(string senderCompID, string senderSubID, string targetCompID, string targetSubID, string customKey = null)
        {
            if (String.IsNullOrWhiteSpace(senderCompID))
                throw new ArgumentNullException(nameof(senderCompID));
            if (String.IsNullOrWhiteSpace(targetCompID))
                throw new ArgumentNullException(nameof(targetCompID));


            this.SenderCompID = senderCompID.Trim().ToUpperInvariant();
            if (!String.IsNullOrWhiteSpace(senderSubID))
                this.SenderSubID = senderSubID.Trim().ToUpperInvariant();

            this.TargetCompID = targetCompID.Trim().ToUpperInvariant();
            if (!String.IsNullOrWhiteSpace(targetSubID))
                this.TargetSubID = targetSubID.Trim().ToUpperInvariant();

            if (!String.IsNullOrWhiteSpace(customKey))
                this.CustomKey = customKey.ToUpperInvariant();

            MakeID();
        }

        void MakeID()
        {
            var sb = new StringBuilder();

            //SenderCompID
            sb.Append(this.SenderCompID);
            //SenderSubID
            if (!string.IsNullOrEmpty(this.SenderSubID))
            {
                sb.Append("_");
                sb.Append(this.SenderSubID);
            }
            //TargetCompID
            sb.Append("_");
            sb.Append(this.TargetCompID);
            //TargetSubID
            if (!string.IsNullOrEmpty(this.TargetSubID))
            {
                sb.Append("_");
                sb.Append(this.TargetSubID);
            }
            //CustomKey
            if (!string.IsNullOrEmpty(this.CustomKey))
            {
                sb.Append("_");
                sb.Append(this.CustomKey);
            }

            this.Id = sb.ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(this.SenderSubID) || string.IsNullOrWhiteSpace(this.TargetSubID))
                return $"{{{this.SenderCompID}, {this.TargetCompID}}}";
            else
                return $"{{{this.SenderCompID}, {this.SenderSubID}, {this.TargetCompID}, {this.TargetSubID}}}";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool Equals(SessionId other)
        {
            if (other is null)
                return false;

            if (Object.ReferenceEquals(this, other))
                return true;


            // If run-time types are not exactly the same, return false.
            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return string.Compare(this.Id, other.Id, ignoreCase: true) == 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) => this.Equals(obj as SessionId);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static Boolean operator ==(SessionId lhs, SessionId rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                return false;
            }

            return lhs.Equals(rhs);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static Boolean operator !=(SessionId lhs, SessionId rhs) => !(lhs == rhs);

    }
}
