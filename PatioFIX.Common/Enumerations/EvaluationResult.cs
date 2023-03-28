namespace PatioFIX.Common
{
    /// <summary>
    /// 
    /// </summary>
    public enum EvaluationResult : short
    {
        Success = 0,
        MessageIgnored = 1,
        RetryableSqlException = 2,
        SqlException = 3,
        Exception = 4,
        FailedOther = 5
    }
}
