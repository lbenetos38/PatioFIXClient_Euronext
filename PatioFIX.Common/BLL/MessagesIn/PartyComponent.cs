using PatioFIX.Common.FixSupport;
using System;

namespace PatioFIX.Common.BLL.MessagesIn
{
    /// <summary>
    /// <Parties> Component Block
    /// The Parties group is used to identify and convey information about 
    /// the entities involved in the financial transaction associated with a FIX message
    /// </summary>
    public class PartyComponent
    {
        bool m_isset;

        /// <summary>
        /// ΜΑς λεει εαν εχει σεταριστει
        /// </summary>
        public bool IsSet => m_isset;

        /// <summary>
        /// PartyID (Tag = 448, Type: String)
        /// </summary>
        public string PartyID;

        /// <summary>
        /// PartyIDSource (Tag = 447, Type: char)
        /// 'D' Proprietary/Custom Code
        /// 'P' Short code identifier
        /// </summary>
        public char PartyIDSource;


        /// <summary>
        /// PartyRole (Tag = 452, Type: int)
        /// </summary>
        public int PartyRole;


        /// <summary>
        /// 
        /// Qualifier of the specified PartyRole (452).
        /// </summary>
        public int PartyRoleQualifier;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        public PartyComponent()
        {
            Reset();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        public PartyComponent(int role)
        {
            Reset();
            PartyRole = role;
        }

        /// <summary>
        /// Διαγραφει ολες τις τιμες του Component εκτος απο το PartyRole!
        /// </summary>
        public void EmptyValues()
        {
            PartyID = string.Empty;
            PartyIDSource = default;
            //To PartyRole δεν το πειραζουμε σε αυτη την περιπτωση
            PartyRoleQualifier = default;
            m_isset = false;
        }
        /// <summary>
        /// Διαγραφει ολες τις τιμες του Component
        /// </summary>
        public void Reset()
        {
            PartyID = string.Empty;
            PartyIDSource = default;
            PartyRole = default;
            PartyRoleQualifier = default;
            m_isset = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="source"></param>
        /// <param name="logger"></param>
        /// <param name="validateDuplicatePartyRole"></param>
        public void Set(FIXMessage message, PartyComponent source, Logger logger, bool validateDuplicatePartyRole)
        {
            if (PartyRole == source.PartyRole)
            {
                if (IsSet)
                {
                    if (validateDuplicatePartyRole)
                    {
                        MetricsProxy.Instance.OnParsingWarning();
                        logger.Warning($"<Parties> Component Block:: PartyRole (452={PartyRole}) DUPLICATE, {message}");
                    }
                    return;
                }

                //this.PartyRole = source.PartyRole;
                PartyIDSource = source.PartyIDSource;
                PartyID = source.PartyID;
                PartyRoleQualifier = source.PartyRoleQualifier;
                m_isset = true;
            }
            else
            {
                throw new Exception($"<Parties> Component Block:: PartyRole (452={PartyRole}) WRONG_SETUP!");
            }
        }


        /// <summary>
        /// Αναθετει την τιμη απο το FixField στο αντιστοιχο πεδία του Component
        /// </summary>
        /// <param name="_field"></param>
        public void SetValue(FIXField _field)
        {
            if (_field.Tag == Tags.PartyRole)
            {
                PartyRole = _field.AsInt;
            }
            else if (_field.Tag == Tags.PartyIDSource)
            {
                PartyIDSource = _field.AsChar;
            }
            else if (_field.Tag == Tags.PartyID)
            {
                PartyID = _field.AsString;
            }
            else if (_field.Tag == CustomTags.PartyRoleQualifier)
            {
                PartyRoleQualifier = _field.AsInt;
            }
        }

        /// <summary>
        /// Μας λεει εαν το συγκεκριμενο property του Component εχει ηδη τιμη
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool HasValue(int tag)
        {
            if (tag == Tags.PartyID)
            {
                if (PartyID != string.Empty)
                    return true;
            }
            else if (tag == Tags.PartyIDSource)
            {
                if (PartyIDSource != default(char))
                    return true;
            }
            else if (tag == Tags.PartyRole)
            {
                if (PartyRole != default)
                    return true;
            }
            else if (tag == CustomTags.PartyRoleQualifier)
            {
                if (PartyRoleQualifier != default)
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Μας λεει εαν το FixField ειναι πεδιο που ανηκει σε 'Party Compoenent'
        /// </summary>
        /// <param name="_field"></param>
        /// <returns></returns>
        public static bool IsFieldRelated(FIXField _field)
        {
            if (
                _field.Tag == Tags.PartyID ||
                _field.Tag == Tags.PartyIDSource ||
                _field.Tag == Tags.PartyRole ||
                _field.Tag == CustomTags.PartyRoleQualifier)
                return true;

            return false;
        }

        public override string ToString()
        {
            if (PartyRole == 1)
                return $"Executing Firm = '{PartyID}'";
            if (PartyRole == 3)
                return $"Client ID = '{PartyID}'";
            if (PartyRole == 4)
                return $"Clearing Firm = '{PartyID}'";
            if (PartyRole == 12)
                return $"Executing Trader = '{PartyID}'";
            if (PartyRole == 17)
                return $"Contra Firm = '{PartyID}'";
            if (PartyRole == 26)
                return $"Correspondent Trader = '{PartyID}'";
            if (PartyRole == 36)
                return $"Entering Trader = '{PartyID}'";
            if (PartyRole == 37)
                return $"Contra Trader = '{PartyID}'";
            if (PartyRole == 122)
                return $"Investment Decision Maker = '{PartyID}'";

            return $"Role={PartyRole}, PartyID={PartyID}";
        }
    }
}
