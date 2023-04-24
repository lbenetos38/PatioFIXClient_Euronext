using PatioFIX.Common.FixSupport;
using System;
using System.Globalization;

namespace PatioFIX.Common.BLL.MessagesIn
{
    /// <summary>
    /// Υποστηριζει ολα τα γνωστα μας Parties που μπορει να συνατησουμε μεσα σε ενα FIX Message
    /// </summary>
    internal class PartiesContainer
    {
        #region private state
        int m_NoPartyIDs;
        PartyComponent m_ExecutingFirm;
        PartyComponent m_ClientIdentificationCode;
        PartyComponent m_ClearingFirm;
        PartyComponent m_ExecutionWithinFirm;
        PartyComponent m_ContraFirm;
        PartyComponent m_NonExecutingBroker;
        PartyComponent m_EnteringTrader;
        PartyComponent m_ContraTrader;
        PartyComponent m_InvestmentDecisionWithinFirm;
        PartyComponent m_tempParty;

        bool m_validateDuplicatePartyRole = false;
        bool m_validateRepeatingGroupEntryCount = false;
        string m_callerName = string.Empty;
        #endregion


        /// <summary>
        /// 
        /// </summary>
        public PartiesContainer(string callerName, bool validateDuplicatePartyRole = false, bool validateRepeatingGroupEntryCount = false)
        {
            m_ExecutingFirm = new PartyComponent(PartyRole.ExecutingFirm);
            m_ClientIdentificationCode = new PartyComponent(PartyRole.ClientID);
            m_ClearingFirm = new PartyComponent(PartyRole.ClearingFirm);
            m_ExecutionWithinFirm = new PartyComponent(PartyRole.ExecutingTrader);
            m_ContraFirm = new PartyComponent(PartyRole.ContraFirm);
            m_NonExecutingBroker = new PartyComponent(PartyRole.CorrespondentBroker);
            m_EnteringTrader = new PartyComponent(PartyRole.EnteringTrader);
            m_ContraTrader = new PartyComponent(PartyRole.ContraTrader);
            m_InvestmentDecisionWithinFirm = new PartyComponent(PartyRole.InvestmentDecisionMaker);
            m_tempParty = new PartyComponent();

            m_callerName = callerName;
            m_validateDuplicatePartyRole = validateDuplicatePartyRole;
            m_validateRepeatingGroupEntryCount = validateRepeatingGroupEntryCount;
            EmptyValues();
        }


        /// <summary>
        /// 
        /// </summary>
        public void EmptyValues()
        {
            m_NoPartyIDs = 0;
            m_ExecutingFirm.EmptyValues();
            m_ClientIdentificationCode.EmptyValues();
            m_ClearingFirm.EmptyValues();
            m_ExecutionWithinFirm.EmptyValues();
            m_ContraFirm.EmptyValues();
            m_NonExecutingBroker.EmptyValues();
            m_EnteringTrader.EmptyValues();
            m_ContraTrader.EmptyValues();
            m_InvestmentDecisionWithinFirm.EmptyValues();

            ClientID = null;
            InvestmentDecisionID = null;
            ExecutionWithinFirmID = null;
            NonExecutingBrokerID = null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="message">Αυτο ειναι το Fix Message απο το οποιο θελουμε να διαβασουμε τα repeating Parties</param>
        /// <param name="logger"></param>
        /// <param name="instance">Eνα fixMessage (παραδειγμα τα Trade Capture Report(ΑΕ)) μπορει να περιεχουν πανω απο μια φορα 
        /// PartiesRepeatingGroups (καθε side μεσα σε ενα Capture Trade Report περιεχει το δικο του Parties Repeating group).
        /// O Caller της μεθοδου ParseParties(), ειναι υπευθυνος να μας πει ποιο instace απο αυτα τα Parties repeating groups 
        /// επιθυμει να διαβασει
        /// </param>
        /// <exception cref="Exception"></exception>
        public void ParseParties(FIXMessage message, Logger logger, int instance = 0)
        {
            m_tempParty.Reset();
            int actualGroupEntryCount = 0;

            /*
             * Αποθηκευουμε την τιμη του NoPartyIDs:
             */
            m_NoPartyIDs = message[Tags.NoPartyIDs, instance].AsInt;       //REQUIRED
            if (m_NoPartyIDs == 0)
            {
                //Δεν εχουμε επαναλαμβανομενα 'Parties Group'
                return;
            }

            /*
             * Θελουμε το index μεσα στον πινακα των FixFields του  message, το οποιο περιεχει
             * το 1o μελος του 1ου 'Party Group':
             */
            int idx = (instance == 0 ? message.IndexOfTag453 : message.GetIndex(453, instance)) + 1;
            /*
             * Περπαταω ολα τα πεδία του message απο το NoPartyIDs και μετα, μεχρι να βρω το πρωτο
             * πεδιο που δεν εχει σχεση με 'Parties Group':
             */
            var field = message.m_fields[idx++];
            while (PartyComponent.IsFieldRelated(field))
            {
                if (m_tempParty.HasValue(field.Tag))
                {
                    SetParty(message, m_tempParty, logger);
                    actualGroupEntryCount++;
                    m_tempParty.Reset();
                }

                m_tempParty.SetValue(field);
                field = message.m_fields[idx++];
            }
            /*
             * Οταν βγαινουμε απο το παραπανω while block, ακομα δεν εχουμε αποθηκευσει το τελευταιο party που μαζευαμε...
             * Ελεγχουμε ομως εαν ειχαμε οντως εστω και ενα
             */
            if (m_tempParty.HasValue(Tags.PartyRole))
            {
                SetParty(message, m_tempParty, logger);
                actualGroupEntryCount++;
            }

            if (m_validateRepeatingGroupEntryCount)
            {
                if (m_NoPartyIDs != actualGroupEntryCount)
                {
                    throw new PtFixException($"ParseParties:: Inconsistency with NoPartyIDs (Tag 453). Excpecting {m_NoPartyIDs}, but actual number is {actualGroupEntryCount}");
                }
            }

            PrepareODLFields(logger);
        }

        void SetParty(FIXMessage message, PartyComponent component, Logger logger)
        {
            if (component.PartyRole == /*Executing Firm*/1)
            {
                m_ExecutingFirm.Set(message, component, logger, m_validateDuplicatePartyRole);
            }
            else if (component.PartyRole == /*Client ID*/3)
            {
                m_ClientIdentificationCode.Set(message, component, logger, m_validateDuplicatePartyRole);
            }
            else if (component.PartyRole == /*Clearing Firm*/4)
            {
                m_ClearingFirm.Set(message, component, logger, m_validateDuplicatePartyRole);
            }
            else if (component.PartyRole == /*Executing Trader - (MIFID II: Execution within firm)*/12)
            {
                m_ExecutionWithinFirm.Set(message, component, logger, m_validateDuplicatePartyRole);
            }
            else if (component.PartyRole == /*Contra Firm*/17)
            {
                m_ContraFirm.Set(message, component, logger, m_validateDuplicatePartyRole);
            }
            else if (component.PartyRole == /*Correspondent Broker - (MIFID II: Non-executing broker)*/26)
            {
                m_NonExecutingBroker.Set(message, component, logger, m_validateDuplicatePartyRole);
            }
            else if (component.PartyRole == /*Entering trader*/36)
            {
                m_EnteringTrader.Set(message, component, logger, m_validateDuplicatePartyRole);
            }
            else if (component.PartyRole == /*Contra Trader*/37)
            {
                m_ContraTrader.Set(message, component, logger, m_validateDuplicatePartyRole);
            }
            else if (component.PartyRole == /*Investment Decision Maker - (MIFID II: Investment decision within firm)*/122)
            {
                m_InvestmentDecisionWithinFirm.Set(message, component, logger, m_validateDuplicatePartyRole);
            }
            else
            {
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"ParseParties:: Unexpected PartyRole (452={component.PartyRole}), {message}");
            }
        }


        void PrepareODLFields(Logger logger)
        {

            #region ClientID, InvestmentDecisionID, ExecutionWithinFirmID, NonExecutingBrokerID Decimal Value initialization
            /*
             * Περιμενω οτι το ClientID θα περιεχει καποιο ακεραιο αριθμο
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Trades -> [TrdClientID] [decimal](18, 0) NULL)
             */
            if (ClientIdentificationCode.IsSet)
            {
                if (decimal.TryParse(ClientIdentificationCode.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _clientID))
                {
                    ClientID = _clientID;
                }
                else
                {
                    ClientID = null;
                    MetricsProxy.Instance.OnParsingWarning();
                    logger.Warning($"{m_callerName} -> ClientIdentificationCode.PartyID '{ClientIdentificationCode.PartyID}' cannot be Parsed as decimal");
                }
            }

            /*
             * Περιμενω οτι το InvestmentDecisionID θα περιεχει καποιο ακεραιο αριθμο
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Trades -> [TrdInvestmentDecisionID] [decimal](18, 0) NULL)
             */
            if (InvestmentDecisionWithinFirm.IsSet)
            {
                if (decimal.TryParse(InvestmentDecisionWithinFirm.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _investmentDecisionID))
                {
                    InvestmentDecisionID = _investmentDecisionID;
                }
                else
                {
                    InvestmentDecisionID = null;
                    MetricsProxy.Instance.OnParsingWarning();
                    logger.Warning($"{m_callerName} -> InvestmentDecisionWithinFirm.PartyID '{InvestmentDecisionWithinFirm.PartyID}' cannot be Parsed as decimal");
                }
            }

            /*
             * Περιμενω οτι το ExecutionWithinFirmID θα περιεχει καποιο ακεραιο αριθμο
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Trades -> [TrdExecutionWithinFirmId] [decimal](18, 0) NULL)
             */
            if (ExecutionWithinFirm.IsSet)
            {
                if (decimal.TryParse(ExecutionWithinFirm.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _executionWithinFirmID))
                {
                    ExecutionWithinFirmID = _executionWithinFirmID;
                }
                else
                {
                    ExecutionWithinFirmID = null;
                    MetricsProxy.Instance.OnParsingWarning();
                    logger.Warning($"{m_callerName} -> ExecutionWithinFirm.PartyID '{ExecutionWithinFirm.PartyID}' cannot be Parsed as decimal");
                }
            }

            /*
             * Περιμενω οτι το NonExecutingBrokerID θα περιεχει καποιο ακεραιο αριθμο
             * Αυτο μας το επιβαλει η βαση μας (ODL.dbo.Trades -> [TrdNonExecutingBrokerID] [decimal](18, 0) NULL)
             */
            if (NonExecutingBroker.IsSet)
            {
                if (decimal.TryParse(NonExecutingBroker.PartyID, NumberStyles.None, NumberFormatInfo.InvariantInfo, out decimal _nonExecutingBrokerID))
                {
                    NonExecutingBrokerID = _nonExecutingBrokerID;
                }
                else
                {
                    NonExecutingBrokerID = null;
                    MetricsProxy.Instance.OnParsingWarning();
                    logger.Warning($"{m_callerName} -> NonExecutingBroker.PartyID '{NonExecutingBroker.PartyID}' cannot be Parsed as decimal");
                }
            }
            #endregion
        }


        #region Parties
        /// <summary>
        /// NoPartyIDs (Tag = 453, Type: NumInGroup)
        /// </summary>
        public int NoPartyIDs => m_NoPartyIDs;

        /// <summary>
        /// 1        Executing Firm
        /// </summary>
        public PartyComponent ExecutingFirm => m_ExecutingFirm;
        /// <summary>
        /// 3 Client ID (MIFID II: Client identification code)
        /// </summary>
        public PartyComponent ClientIdentificationCode => m_ClientIdentificationCode;
        /// <summary>
        /// 4 Clearing Firm
        /// </summary>
        public PartyComponent ClearingFirm => m_ClearingFirm;
        /// <summary>
        /// 12 Executing trader (MIFID II: Execution within firm)
        /// </summary>
        public PartyComponent ExecutionWithinFirm => m_ExecutionWithinFirm;
        /// <summary>
        /// 17 Contra Firm
        /// </summary>
        public PartyComponent ContraFirm => m_ContraFirm;
        /// <summary>
        /// 26 Correspondent broker (MIFID II: Non-executing broker)
        /// </summary>
        public PartyComponent NonExecutingBroker => m_NonExecutingBroker;
        /// <summary>
        /// 36 Entering trader (Trader ID)
        /// </summary>
        public PartyComponent EnteringTrader => m_EnteringTrader;
        /// <summary>
        /// 37 Contra Trader
        /// </summary>
        public PartyComponent ContraTrader => m_ContraTrader;
        /// <summary>
        /// 122 Investment Decision Maker (MIFID II: Investment decision within firm)
        /// </summary>
        public PartyComponent InvestmentDecisionWithinFirm => m_InvestmentDecisionWithinFirm;
        #endregion


        /// <summary>
        /// ClientIdentificationCode.PartyID
        /// </summary>
        public decimal? ClientID { get; private set; }
        public char ClientIDQualifier
        {
            get
            {
                var qualifier = ClientIdentificationCode.PartyRoleQualifier;

                if (qualifier == /*Firm or legal entity*/23)
                    return 'L';//76
                else if (qualifier == /*Natural person*/24)
                    return 'N';//78
                else
                    return 'N';//78
            }
        }

        /// <summary>
        /// ExecutionWithinFirm
        /// </summary>
        public decimal? ExecutionWithinFirmID { get; private set; }
        public char ExecutionWithinFirmIDQualifier
        {
            get
            {
                var qualifier = ExecutionWithinFirm.PartyRoleQualifier;

                if (qualifier == /*Algorithm*/22)
                    return 'A';//65
                else if (qualifier == /*Natural person*/24)
                    return 'N';//78
                else
                    return 'X';//78
            }
        }

        /// <summary>
        /// NonExecutingBroker
        /// </summary>
        public decimal? NonExecutingBrokerID { get; private set; }

        /// <summary>
        /// InvestmentDecisionWithinFirm
        /// </summary>
        public decimal? InvestmentDecisionID { get; private set; }
        public char InvestmentDecisionIDQualifier
        {
            get
            {
                var qualifier = InvestmentDecisionWithinFirm.PartyRoleQualifier;

                if (qualifier == /*Algorithm*/22)
                    return 'A';//65
                else if (qualifier == /*Natural person*/24)
                    return 'N';//78
                else
                    return 'X';//78
            }
        }


    }
}
