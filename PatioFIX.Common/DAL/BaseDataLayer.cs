using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace PatioFIX.Common.DAL
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class BaseDataLayer
    {

        /// <summary>
        /// Αυτό είναι το τρέχων Connection String
        /// </summary>
        internal string ConnectionString { get; private set; }


        /// <summary>
        /// Initialize a DataAccess kind of class.
        /// <para>The first think to do is to setup connection string</para>
        /// </summary>
        public BaseDataLayer(string connString)
        {
            if (string.IsNullOrWhiteSpace(connString))
            {
                throw new ArgumentNullException(nameof(connString));
            }
            this.ConnectionString = connString;
        }


        /// <summary>
        /// Initializes a new instance of the SqlConnection class.
        /// </summary>
        /// <returns></returns>
        protected SqlConnection CreateConnection()
        {
            var conn = new SqlConnection
            {
                ConnectionString = ConnectionString
            };

            return conn;
        }
        /// <summary>
        /// Creates and returns a SqlCommand object associated with the connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTimeout">The time in seconds to wait for the command to execute. The default is 30 seconds.</param>
        /// <returns></returns>
        protected SqlCommand CreateCommand(SqlConnection connection = null, SqlTransaction transaction = null, CommandType commandType = CommandType.StoredProcedure, Int32 commandTimeout = 30)
        {
            SqlConnection conn = connection ?? CreateConnection();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandType = commandType;
            cmd.Transaction = transaction;
            cmd.CommandTimeout = commandTimeout;
            return cmd;
        }
        /// <summary>
        /// Creates a SqlCommand ready to 'call' the storeProcedureName
        /// </summary>
        /// <param name="storeProcedureName"></param>
        /// <returns></returns>
        protected SqlCommand CreateCommandForProc(string storeProcedureName)
        {
            SqlCommand cmd = CreateCommand(commandType: CommandType.StoredProcedure);
            cmd.CommandText = storeProcedureName;
            return cmd;
        }
        /// <summary>
        /// Creates a SqlCommand ready to 'execute' the sql
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        protected SqlCommand CreateCommandForSql(string sql)
        {
            SqlCommand cmd = CreateCommand(commandType: CommandType.Text);
            cmd.CommandText = sql;
            return cmd;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <param name="parameterType"></param>
        /// <returns></returns>
        protected SqlParameter AddParameter(SqlCommand cmd, string parameterName, object parameterValue, SqlDbType parameterType)
        {
            SqlParameter p1 = cmd.CreateParameter();
            p1.ParameterName = parameterName;
            if (parameterValue == null)
                p1.Value = DBNull.Value;
            else
                p1.Value = parameterValue;
            p1.SqlDbType = parameterType;

            return cmd.Parameters.Add(p1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <param name="parameterType"></param>
        /// <param name="parameterDirection"></param>
        /// <returns></returns>
        protected SqlParameter AddParameter(SqlCommand cmd, string parameterName, object parameterValue, SqlDbType parameterType, ParameterDirection parameterDirection)
        {
            SqlParameter p1 = cmd.CreateParameter();
            p1.ParameterName = parameterName;
            if (parameterValue == null)//if ((parameterValue.HasValue == false))
                p1.Value = DBNull.Value;
            else
                p1.Value = parameterValue;
            p1.SqlDbType = parameterType;
            p1.Direction = parameterDirection;
            cmd.Parameters.Add(p1);
            return p1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <param name="parameterType"></param>
        /// <param name="parameterDirection"></param>
        /// <param name="parameterSize"></param>
        /// <returns></returns>
        protected SqlParameter AddParameter(SqlCommand cmd, string parameterName, object parameterValue, SqlDbType parameterType, ParameterDirection parameterDirection, int parameterSize)
        {
            SqlParameter p1 = cmd.CreateParameter();
            p1.ParameterName = parameterName;
            if (parameterValue == null)//if ((parameterValue.HasValue == false))
                p1.SqlValue = DBNull.Value;
            else
                p1.SqlValue = parameterValue;
            p1.SqlDbType = parameterType;
            p1.Direction = parameterDirection;
            p1.Size = parameterSize;
            cmd.Parameters.Add(p1);
            return p1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <param name="parameterDirection"></param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        protected SqlParameter AddDecimalParameter(SqlCommand cmd, string parameterName, object parameterValue, ParameterDirection parameterDirection, byte precision = 18, byte scale = 2)
        {
            SqlParameter p1 = cmd.CreateParameter();
            p1.ParameterName = parameterName;
            if (parameterValue == null)//if ((parameterValue.HasValue == false))
                p1.SqlValue = DBNull.Value;
            else
                p1.SqlValue = parameterValue;
            p1.SqlDbType = SqlDbType.Decimal;
            p1.Direction = parameterDirection;
            p1.Precision = precision;
            p1.Scale = scale;
            cmd.Parameters.Add(p1);
            return p1;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected DataTable ExecuteReader(SqlCommand cmd)
        {
            DataTable dtable = null;
            bool _closeTheConnection = false;

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                    _closeTheConnection = true;
                }
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    dtable = new DataTable();
                    dtable.Load(reader);
                }
            }
            catch
            {
                if (dtable != null)
                {
                    dtable.Dispose();
                }
                throw;
            }
            finally
            {
                if (_closeTheConnection)
                {
                    cmd.Connection.Close();
                }
            }

            return dtable;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nativeSqlStm"></param>
        /// <returns></returns>
        protected int ExecuteNativeSql(string nativeSqlStm)
        {
            if (nativeSqlStm == null) throw new ArgumentNullException(nameof(nativeSqlStm));

            SqlCommand command = CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = nativeSqlStm;
            return ExecuteNonReader(command);
        }
        /// <summary>
        /// Executes a SQL command and returns the number of rows affected.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected int ExecuteNonReader(SqlCommand cmd)
        {
            int returnValue = 0;
            bool _closeTheConnection = false;

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                    _closeTheConnection = true;
                }
                returnValue = cmd.ExecuteNonQuery();
            }
            finally
            {
                if (_closeTheConnection)
                {
                    cmd.Connection.Close();
                }
            }

            return returnValue;
        }
        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected Object ExecuteScalar(SqlCommand cmd)
        {
            Object returnValue = null;
            bool _closeTheConnection = false;

            try
            {
                if (cmd.Connection.State == ConnectionState.Closed)
                {
                    cmd.Connection.Open();
                    _closeTheConnection = true;
                }
                returnValue = cmd.ExecuteScalar();
            }
            finally
            {
                if (_closeTheConnection)
                {
                    cmd.Connection.Close();
                }
            }

            return returnValue;
        }


        /// <summary>
        /// 
        /// </summary>
        static protected string _direction(string sql, PtSortDirection direction)
        {
            if (direction == PtSortDirection.Desc)
            {
                return sql += " desc";
            }
            return sql;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="ex"></param>
        /// <param name="method"></param>
        /// <param name="values">Οι παράμετροι κλήσης της μεθόδου έτσι όπως είναι ορισμένοι (με την ίδια σειρά)</param>
        protected void LogDeadlock(Logger logger, Exception ex, int retryCount, MethodBase method, params object[] values)
        {
            #region φτιάχνουμε την μέθοδο και τις παραμέτρους:
            ParameterInfo[] parms = method.GetParameters();
            object[] namevalues = new object[2 * parms.Length];

            string msg = method.Name + "(";
            for (int i = 0, j = 0; i < parms.Length; i++)
            {
                if (i > 0)
                    msg += ", ";
                if (parms[i].Name == "authUser")
                {
                    msg += "'{" + j + "}'";
                    if (i < values.Length) namevalues[j] = values[i]; else namevalues[j] = "??";
                    j += 1;
                }
                else
                {
                    msg += "{" + j + "}='{" + (j + 1) + "}'";
                    namevalues[j] = parms[i].Name;
                    if (i < values.Length) namevalues[j + 1] = values[i]; else namevalues[j + 1] = "??";
                    j += 2;
                }
            }
            msg += ")";
            #endregion

            logger.Warning(string.Format("DEADLOCK at {0}, (retryCount = {1})", string.Format(msg, namevalues), retryCount));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="ex"></param>
        /// <param name="method"></param>
        /// <param name="values">Οι παράμετροι κλήσης της μεθόδου έτσι όπως είναι ορισμένοι (με την ίδια σειρά)</param>
        protected void LogException(Logger logger, Exception ex, MethodBase method, params object[] values)
        {
            #region φτιάχνουμε την μέθοδο και τις παραμέτρους:
            ParameterInfo[] parms = method.GetParameters();
            object[] namevalues = new object[2 * parms.Length];

            string msg = method.Name + "(";
            for (int i = 0, j = 0; i < parms.Length; i++)
            {
                if (i > 0)
                    msg += ", ";
                if (parms[i].Name == "authUser")
                {
                    msg += "'{" + j + "}'";
                    if (i < values.Length) namevalues[j] = values[i]; else namevalues[j] = "??";
                    j += 1;
                }
                else
                {
                    msg += "{" + j + "}='{" + (j + 1) + "}'";
                    namevalues[j] = parms[i].Name;
                    if (i < values.Length) namevalues[j + 1] = values[i]; else namevalues[j + 1] = "??";
                    j += 2;
                }
            }
            msg += ")";
            #endregion


            if (ex.GetType() == typeof(PtInvalidAuthUserException))
            {
                logger.Error("INVALID_ACCESSTOKEN at " + string.Format(msg, namevalues));
            }
            else if (ex.GetType() == typeof(PtTimeOutException))
            {
                logger.Warning("EXECUTION_TIMEOUT_EXPIRED at " + string.Format(msg, namevalues));
            }
            else if (ex.GetType() == typeof(PtDeadlockException))
            {
                logger.Error("DEADLOCK at " + string.Format(msg, namevalues));
            }
            else if (ex.GetType() == typeof(SqlException))
            {
                logger.Error(string.Format("SqlException ({0})", ex.Message));
                logger.Error(string.Format(msg, namevalues));
            }
            else
            {
                if (ex.Message == "CANNOT_UPDATE_BUILTIN_ENTITY" ||
                        ex.Message == "CANNOT_DELETE_BUILTIN_ENTITY" ||
                        ex.Message == "RECORD_NOT_EXIST" ||
                        ex.Message == "RECORD_CHANGED" ||
                        ex.Message == "CANNOT_BE_DELETED" ||
                        ex.Message == "CANNOT_BE_UPDATED" ||
                        ex.Message == "NAME_ALREADY_EXISTS"
                   )
                {
                    logger.Error(ex.Message + " at " + string.Format(msg, namevalues));
                }
                else
                {
                    logger.Error(string.Format(msg, namevalues));
                    LogException(logger, ex);
                }
            }
        }

        /// <summary>
        /// Here we log the exceptions, but before that we reverse their order:
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="ex"></param>
        protected void LogException(Logger logger, Exception ex)
        {
            Stack<Exception> exceptions = new Stack<Exception>();
            while (ex != null)
            {
                exceptions.Push(ex);
                ex = ex.InnerException;
            }

            while (exceptions.Count > 0)
            {
                var _ex = exceptions.Pop();

                if (_ex.GetType() == typeof(SqlException))
                {
                    SqlException sqlEx = (SqlException)_ex;
                    string message;

                    if (!string.IsNullOrWhiteSpace(sqlEx.Procedure))
                    {
                        message = string.Format("{0}, Server='{1}', Procedure='{2}'", sqlEx.Message, sqlEx.Server, sqlEx.Procedure);
                    }
                    else
                    {
                        message = string.Format("{0}, Server='{1}'", sqlEx.Message, sqlEx.Server);
                    }
                    logger.Error(message);
                    logger.Error(_ex);
                }
                else
                {
                    logger.Error(ex);
                }
            }
        }


    }
}
