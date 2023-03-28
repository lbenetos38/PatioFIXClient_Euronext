namespace PatioFIX.Common
{
    public static class Helpers
    {
        public static string GetATHEXServerCode(ATHEXServerEnum value)
        {
            switch (value)
            {
                case ATHEXServerEnum.DSS:
                    return "DSS";
                case ATHEXServerEnum.ORA:
                    return "ORA";
                default:
                    return "ETS";
            }
        }


        public static ATHEXServerEnum GetATHEXServerEnum(string bsConnection)
        {
            if (bsConnection != null && bsConnection.Length == 3)
            {
                if (bsConnection[0] == 'D' && bsConnection[1] == 'S' && bsConnection[2] == 'S')
                    return ATHEXServerEnum.DSS;
                if (bsConnection[0] == 'O' && bsConnection[1] == 'R' && bsConnection[2] == 'A')
                    return ATHEXServerEnum.ORA;
            }

            return ATHEXServerEnum.ETS;
        }
    }
}
