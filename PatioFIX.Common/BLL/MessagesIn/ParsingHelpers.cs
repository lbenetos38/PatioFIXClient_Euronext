using PatioFIX.Common.FixSupport;

namespace PatioFIX.Common.BLL.Messages
{

    internal static class ParsingHelpers
    {
        /// <summary>
        /// OrderCapacity (Tag = 528, Type: char)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static char GetOrderCapacityAsTradingCapacity(FIXMessage message, Logger logger)
        {
            if (message.Contains(Tags.OrderCapacity))
            {
                var _orderCapacity = message[Tags.OrderCapacity].AsChar;
                if (_orderCapacity == 'A')
                {
                    //Agency (AOTC)
                    return '2';
                }
                else if (_orderCapacity == 'P')
                {
                    //Principal (DEAL)
                    return '1';
                }
                else if (_orderCapacity == 'R')
                {
                    //Riskless principal (MTCH)
                    return '0';
                }
                else
                {
                    MetricsProxy.Instance.OnParsingError();
                    throw new PtFixException($"tag528 (OrderCapacity) has UNSUPPORTED_VALUE of '{_orderCapacity}'");
                }
            }
            else
            {
                return '2';
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        /// <exception cref="PtFixException"></exception>
        public static char GetLastLiquidityInd(FIXMessage message, Logger logger)
        {
            var _LastLiquidityInd = message[Tags.LastLiquidityInd].AsInt;
            if (_LastLiquidityInd == /*Added Liquidity*/1)
            {
                return 'A';
            }
            else if (_LastLiquidityInd == /*Removed Liquidity*/2)
            {
                return 'R';
            }
            else if (_LastLiquidityInd == /*Auction*/4)
            {
                return 'N';
            }
            else
            {
                MetricsProxy.Instance.OnParsingError();
                throw new PtFixException($"tag851 (LastLiquidityInd) has UNSUPPORTED_VALUE of '{_LastLiquidityInd}'");
            }
        }


        public static char GetOrderRelFlag(FIXMessage message, Logger logger)
        {
            if (message.Contains(CustomTags.OrderRelFlag))
            {
                var _value = message[CustomTags.OrderRelFlag].AsInt;

                if (_value == /*Normal*/1)
                {
                    return 'N';
                }
                else if (_value == /*Quote*/2)
                {
                    return 'Q';
                }
                else if (_value == /*Combo*/3)
                {
                    return 'C';
                }


                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"tag5509 (OrderRelFlag) has UNSUPPORTED_VALUE of '{_value}'");

                return 'N';
            }
            else
            {
                MetricsProxy.Instance.OnParsingWarning();
                logger.Warning($"tag5509 (OrderRelFlag) NOT_FOUND'");

                return 'N';
            }

        }


        public static char GetOrderOrigination(FIXMessage message, Logger logger)
        {
            var _orderOrigination = message[CustomTags.OrderOrigination].AsChar;

            if (_orderOrigination == '0')
            {   //Order is not submitted using Direct Electronic Access (DEA)
                return '0';
            }
            else if (_orderOrigination == '5')
            {   //Order is submitted using Direct Electronic Access (DEA)
                return '1';
            }
            else
            {
                MetricsProxy.Instance.OnParsingError();
                throw new PtFixException($"tag1724 (OrderOrigination) has UNSUPPORTED_VALUE of '{_orderOrigination}'");
            }
        }

        public static char GetOrdType(FIXMessage message, Logger logger, bool warnIfNotExists = true)
        {
            if (message.Contains(Tags.OrdType))
            {
                var _ordType = message[Tags.OrdType].AsChar;
                if (_ordType == /*Market*/'1')
                {
                    return 'M';
                }
                else if (_ordType == /*Limit or Better*/'7')
                {
                    return 'L';
                }
                else if (_ordType == /*Stop*/'3')
                {
                    return 'S';
                }
                else if (_ordType == /*Stop limit*/'4')
                {
                    return 'T';
                }
                else if (_ordType == /*On Close*/'A')
                {
                    return 'C';
                }
                else if (_ordType == /*Hit & take order selection*/'Q')
                {
                    return 'Q';
                }
                else
                {
                    MetricsProxy.Instance.OnParsingError();
                    throw new PtFixException($"tag40 (OrdType) has UNSUPPORTED_VALUE of '{_ordType}'");
                }
            }
            else
            {
                if (warnIfNotExists == true)
                {
                    MetricsProxy.Instance.OnParsingWarning();
                    logger.Warning($"tag40 (OrdType) NOT_FOUND, {message}");
                }

                return default(char);
            }
        }

        /// <summary>
        /// Μετατρεπουμε το TimeInForce (Tag = 59, Type: char) στο πεδιο
        /// OrderLifeTime του ODL protocol
        /// </summary>
        /// <param name="message"></param>
        /// <param name="logger"></param>
        /// <param name="warnIfNotExists"></param>
        /// <returns></returns>
        /// <exception cref="PtFixException"></exception>
        public static char GetTimeInForce(FIXMessage message, Logger logger, bool warnIfNotExists = true)
        {
            /*
             * Το OrderLifeTime στο ODL ειχε μονο 3 τιμες:
             *  D -> for orders that if they remain not executed or partially not executed until the end of the current day’s trading session they will be cancelled by the system
             *  C-> for orders that remain in the orders book (for the upcoming days trading sessions as well) until fully executed or cancelled by the Member
             *  E -> for orders that will remain active in the orders book until they are executed or up until the date set in the ExpirationDate property
             */
            if (message.Contains(Tags.TimeInForce))
            {
                var _timeInForce = message[Tags.TimeInForce].AsChar;
                if (_timeInForce == /*Day (or session)*/'0')
                {
                    return 'D';/*ταιριαζει 100% με το OrderLifeTime*/
                }
                else if (_timeInForce == /*Good Till Cancel (GTC)*/'1')
                {
                    return 'C';/*ταιριαζει 100% με το OrderLifeTime*/
                }
                else if (_timeInForce == /*At the Opening (OPG)*/'2')
                {
                    return 'D';/**/
                }
                else if (_timeInForce == /*Immediate Or Cancel (IOC)*/'3')
                {
                    return 'D';/**/
                }
                else if (_timeInForce == /*Fill Or Kill (FOK)*/'4')
                {
                    return 'D';/**/
                }
                else if (_timeInForce == /*Good Till Crossing (GTX)*/'5')
                {
                    return 'D';/**/
                }
                else if (_timeInForce == /*Good Till Date*/'6')
                {
                    return 'E';/*ταιριαζει 100% με το OrderLifeTime*/
                }
                else if (_timeInForce == /*At the Close*/'7')
                {
                    return 'D';/**/
                }
                else
                {
                    MetricsProxy.Instance.OnParsingError();
                    throw new PtFixException($"tag59 (TimeInForce) has UNSUPPORTED_VALUE of '{_timeInForce}'");
                }
            }
            else
            {
                if (warnIfNotExists == true)
                {
                    MetricsProxy.Instance.OnParsingWarning();
                    logger.Warning($"tag59 (TimeInForce) NOT_FOUND, {message}");
                }

                return default(char);
            }
        }

        /*
         * Απο το FIX OrdStatus (Tag = 39, Type: char),
         * παίρνουμε το παλιο ODL OrderStatus
         */
        public static string map_FIXOrdStatus_To_ODLOrderStatus(char fix_ordStatus)
        {
            /*
             * TO ODL Protocol είχε τα παρακάτω statuses:
             * 
             * A 2-character alphanumeric field indicating the status of an order. Possible values :
             * “  “ Not available (*)
             * “N “ Not Released
             * “I “ Inactive
             * “O ” Open
             * “M ” Match
             * “X ” Cancel
             * “EP” GTC, GTD expired status
             * “A ” Pending for approval
             * 
             */
            if (fix_ordStatus == OrdStatus.New)
            {
                return "O";//Open
            }
            else if (fix_ordStatus == OrdStatus.PartiallyFilled)
            {
                return "O";//Open
            }
            else if (fix_ordStatus == OrdStatus.Filled)
            {
                return "M";//Match
            }
            else if (fix_ordStatus == OrdStatus.Canceled)
            {
                return "X";//Cancel
            }
            if (fix_ordStatus == OrdStatus.Rejected)
            {
                return "8";// Rejected
            }
            else if (fix_ordStatus == OrdStatus.Suspended)
            {
                return "S";//Suspended
            }
            else if (fix_ordStatus == OrdStatus.Expired)
            {
                return "EP";//GTC, GTD expired status
            }
            else if (fix_ordStatus == OrdStatus.Inactive)
            {
                return "I";//Inactive
            }
            else if (fix_ordStatus == OrdStatus.Not_Released)
            {
                return "N";//Not released
            }
            else
            {
                return fix_ordStatus.ToString();
            }
        }
    }
}
