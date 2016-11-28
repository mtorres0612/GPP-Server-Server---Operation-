using System;
using System.Collections;
using System.Xml;
using System.Data;
using System.Data.SqlClient;

namespace IAPL.Transport.Data.Sql
{
	/// <summary>
	/// SqlAccessor class.
	/// </summary>
	public class SqlAccessor
	{
		private static int commandTimeOut = 30;

		private SqlAccessor()
		{
		}

		#region Properties
		public static int CommandTimeOut
		{
			get
			{
				return commandTimeOut;
			}
			set
			{
				commandTimeOut = value;
			}
		}
		#endregion

		#region Methods

		#region ExecuteDataSet Overload
		public static DataSet ExecuteDataSet(SqlConnection connection , SqlCommand command , CommandType commandType , SqlTransaction transaction , string srcTable)
		{
			bool isConnectionClose = (connection.State != ConnectionState.Open);
			DataSet ds = null;
			Exception exThrown = null;
			try
			{
				if (isConnectionClose) { connection.Open(); }
				if (command.Connection == null) { command.Connection = connection; }
				if (transaction != null) { command.Transaction = transaction; }
				command.CommandType = commandType;
				command.CommandTimeout = commandTimeOut;
				SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
				ds = new DataSet();
				if (srcTable == "" || srcTable.Equals(String.Empty))
				{
					dataAdapter.Fill(ds);
				}
				else
				{
					dataAdapter.Fill(ds, srcTable);
				}
			}
			catch (Exception exc)
			{
				exThrown = exc;
			}
			finally
			{
				if (connection.State != ConnectionState.Closed && isConnectionClose)
				{
					connection.Close();
					connection.Dispose();
				}
			}
			if (exThrown != null) { throw exThrown; }
			return ds;
		}

		public static DataSet ExecuteDataSet(string connectionString , SqlCommand command , CommandType commandType , string srcTable)
		{
			return ExecuteDataSet(new SqlConnection(connectionString), command, commandType, null, srcTable);
		}

		public static DataSet ExecuteDataSet(SqlConnection connection , string commandText , CommandType commandType , SqlTransaction transaction , string srcTable)
		{
			return ExecuteDataSet(connection, new SqlCommand(commandText, connection), commandType, transaction, srcTable);
		}

		public static DataSet ExecuteDataSet(string connectionString, string commandText, CommandType commandType, string srcTable)
		{
			return ExecuteDataSet(new SqlConnection(connectionString), new SqlCommand(commandText), commandType, null, srcTable);
		}

		public static DataSet ExecuteDataSet(SqlConnection connection, SqlCommand command, object[] parameterValues, CommandType commandType, SqlTransaction transaction, string srcTable)
		{
			return ExecuteDataSet(connection, SqlCommandBuilder(command, parameterValues), commandType, transaction, srcTable);
		}

		public static DataSet ExecuteDataSet(string connectionString, SqlCommand command, object[] parameterValues, CommandType commandType, string srcTable)
		{
			return ExecuteDataSet(new SqlConnection(connectionString), SqlCommandBuilder(command, parameterValues), commandType, null, srcTable);
		}

		public static DataSet ExecuteDataSet(SqlConnection connection, SqlCommand command, SqlParameter[] parameters, CommandType commandType, SqlTransaction transaction, string srcTable)
		{
			return ExecuteDataSet(connection, SqlCommandBuilder(command, parameters), commandType, transaction, srcTable);
		}

		public static DataSet ExecuteDataSet(string connectionString, SqlCommand command, SqlParameter[] parameters, CommandType commandType, string srcTable)
		{
			return ExecuteDataSet(new SqlConnection(connectionString), SqlCommandBuilder(command, parameters), commandType, null, srcTable);
		}

		public static DataSet ExecuteDataSet(SqlConnection connection, string commandText, SqlParameter[] parameters, CommandType commandType, SqlTransaction transaction, string srcTable)
		{
			return ExecuteDataSet(connection, SqlCommandBuilder(new SqlCommand(commandText, connection), parameters), commandType, transaction, srcTable);
		}

		public static DataSet ExecuteDataSet(string connectionString, string commandText, SqlParameter[] parameters, CommandType commandType, string srcTable)
		{
			return ExecuteDataSet(new SqlConnection(connectionString), SqlCommandBuilder(new SqlCommand(commandText), parameters), commandType, null, srcTable);
		}

		public static DataSet ExecuteDataSet(SqlConnection connection, SqlCommand command, string[] parameterNames, object[] parameterValues, CommandType commandType, SqlTransaction transaction, string srcTable)
		{
			return ExecuteDataSet(connection, SqlCommandBuilder(command, parameterNames, parameterValues), commandType, transaction, srcTable);
		}

		public static DataSet ExecuteDataSet(string connectionString, SqlCommand command, string[] parameterNames, object[] parameterValues, CommandType commandType, string srcTable)
		{
			return ExecuteDataSet(new SqlConnection(connectionString), SqlCommandBuilder(command, parameterNames, parameterValues), commandType, null, srcTable);
		}

		public static DataSet ExecuteDataSet(SqlConnection connection, string commandText, string[] parameterNames, object[] parameterValues, CommandType commandType, SqlTransaction transaction, string srcTable)
		{
			return ExecuteDataSet(connection, SqlCommandBuilder(new SqlCommand(commandText, connection), parameterNames, parameterValues), commandType, transaction, srcTable);
		}

		public static DataSet ExecuteDataSet(string connectionString, string commandText, string[] parameterNames, object[] parameterValues, CommandType commandType, string srcTable)
		{
			return ExecuteDataSet(new SqlConnection(connectionString), SqlCommandBuilder(new SqlCommand(commandText), parameterNames, parameterValues), commandType, null, srcTable);
		}
		#endregion

		#region ExecuteNonQuery Overload
		public static int ExecuteNonQuery(SqlConnection connection, SqlCommand command, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			bool isConnectionClose = (connection.State != ConnectionState.Open);
			int r = -1;
			Exception exThrown = null;
			returnValues = null;
			try
			{
				if (isConnectionClose) { connection.Open(); }
				if (command.Connection == null) { command.Connection = connection; }
				if (transaction != null) { command.Transaction = transaction; }
				command.CommandType = commandType;
				command.CommandTimeout = commandTimeOut;
				r = command.ExecuteNonQuery();
				returnValues = GetReturnValues(command);
			}
			catch (Exception exc)
			{
				exThrown = exc;
			}
			finally
			{
				if (connection.State != ConnectionState.Closed && isConnectionClose) {
					connection.Close();
					connection.Dispose();
				}
			}
			if (exThrown != null) { throw exThrown; }
			return r;
		}

		public static int ExecuteNonQuery(string connectionString, SqlCommand command, CommandType commandType, out object[] returnValues)
		{
			return ExecuteNonQuery(new SqlConnection(connectionString), command, commandType, null, out returnValues);
		}

		public static int ExecuteNonQuery(SqlConnection connection, string commandText, CommandType commandType, out object[] returnValues)
		{
			return ExecuteNonQuery(connection, new SqlCommand(commandText, connection), commandType, null, out returnValues);
		}

		public static int ExecuteNonQuery(string connectionString, string commandText, CommandType commandType, out object[] returnValues)
		{
			return ExecuteNonQuery(new SqlConnection(connectionString), new SqlCommand(commandText), commandType, null, out returnValues);
		}

		public static int ExecuteNonQuery(SqlConnection connection, SqlCommand command, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteNonQuery(connection, SqlCommandBuilder(command, parameterValues), commandType, transaction, out returnValues);
		}

		public static int ExecuteNonQuery(string connectionString, SqlCommand command, object[] parameterValues, CommandType commandType, out object[] returnValues)
		{
			return ExecuteNonQuery(new SqlConnection(connectionString), SqlCommandBuilder(command, parameterValues), commandType, null, out returnValues);
		}

		public static int ExecuteNonQuery(SqlConnection connection, SqlCommand command, SqlParameter[] parameters, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteNonQuery(connection, SqlCommandBuilder(command, parameters), commandType, transaction, out returnValues);
		}

		public static int ExecuteNonQuery(string connectionString, SqlCommand command, SqlParameter[] parameters, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteNonQuery(new SqlConnection(connectionString), SqlCommandBuilder(command, parameters), commandType, transaction, out returnValues);
		}

		public static int ExecuteNonQuery(SqlConnection connection, string commandText, SqlParameter[] parameters, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteNonQuery(connection, SqlCommandBuilder(new SqlCommand(commandText, connection), parameters), commandType, transaction, out returnValues);
		}

		public static int ExecuteNonQuery(string connectionString, string commandText, SqlParameter[] parameters, CommandType commandType, out object[] returnValues)
		{
			return ExecuteNonQuery(new SqlConnection(connectionString), SqlCommandBuilder(new SqlCommand(commandText), parameters), commandType, null, out returnValues);
		}

		public static int ExecuteNonQuery(SqlConnection connection, SqlCommand command, string[] parameterNames, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteNonQuery(connection, SqlCommandBuilder(command, parameterNames, parameterValues), commandType, transaction, out returnValues);
		}

		public static int ExecuteNonQuery(string connectionString, SqlCommand command, string[] parameterNames, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteNonQuery(new SqlConnection(connectionString), SqlCommandBuilder(command, parameterNames, parameterValues), commandType, transaction, out returnValues);
		}

		public static int ExecuteNonQuery(SqlConnection connection, string commandText, string[] parameterNames, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteNonQuery(connection, SqlCommandBuilder(new SqlCommand(commandText, connection), parameterNames, parameterValues), commandType, transaction, out returnValues);
		}

		public static int ExecuteNonQuery(string connectionString, string commandText, string[] parameterNames, object[] parameterValues, CommandType commandType, out object[] returnValues)
		{
			return ExecuteNonQuery(new SqlConnection(connectionString), SqlCommandBuilder(new SqlCommand(commandText), parameterNames, parameterValues), commandType, null, out returnValues);
		}
		#endregion

		#region ExecuteReader Overload
		public static SqlDataReader ExecuteReader(SqlConnection connection, SqlCommand command, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			bool isConnectionClose = (connection.State != ConnectionState.Open);
			returnValues = null;
			try
			{
				if (isConnectionClose) { connection.Open(); }
				if (command.Connection == null) { command.Connection = connection; }
				if (transaction != null) { command.Transaction = transaction; }
				command.CommandType = commandType;
				command.CommandTimeout = commandTimeOut;
				SqlDataReader reader;
				if (isConnectionClose)
				{
					reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				}
				else
				{
					reader = command.ExecuteReader();
				}
				returnValues = GetReturnValues(command);
				return reader;
			}
			catch (Exception exc)
			{
				if (connection.State != ConnectionState.Closed && isConnectionClose)
				{
					connection.Close();
					connection.Dispose();
				}
				throw exc;
			}
		}

		public static SqlDataReader ExecuteReader(string connectionString, SqlCommand command, CommandType commandType, out object[] returnValues)
		{
			return ExecuteReader(new SqlConnection(connectionString), command, commandType, null, out returnValues);
		}

		public static SqlDataReader ExecuteReader(SqlConnection connection, string commandText, CommandType commandType, out object[] returnValues)
		{
			return ExecuteReader(connection, new SqlCommand(commandText, connection), commandType, null, out returnValues);
		}

		public static SqlDataReader ExecuteReader(string connectionString, string commandText, CommandType commandType, out object[] returnValues)
		{
			return ExecuteReader(new SqlConnection(connectionString), new SqlCommand(commandText), commandType, null, out returnValues);
		}

		public static SqlDataReader ExecuteReader(SqlConnection connection, SqlCommand command, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteReader(connection, SqlCommandBuilder(command, parameterValues), commandType, transaction, out returnValues);
		}

		public static SqlDataReader ExecuteReader(string connectionString, SqlCommand command, object[] parameterValues, CommandType commandType, out object[] returnValues)
		{
			return ExecuteReader(new SqlConnection(connectionString), SqlCommandBuilder(command, parameterValues), commandType, null, out returnValues);
		}

		public static SqlDataReader ExecuteReader(SqlConnection connection, SqlCommand command, SqlParameter[] parameters, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteReader(connection, SqlCommandBuilder(command, parameters), commandType, transaction, out returnValues);
		}

		public static SqlDataReader ExecuteReader(string connectionString, SqlCommand command, SqlParameter[] parameters, CommandType commandType, out object[] returnValues)
		{
			return ExecuteReader(new SqlConnection(connectionString), SqlCommandBuilder(command, parameters), commandType, null, out returnValues);
		}

		public static SqlDataReader ExecuteReader(SqlConnection connection, string commandText, SqlParameter[] parameters, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteReader(connection, SqlCommandBuilder(new SqlCommand(commandText, connection), parameters), commandType, transaction, out returnValues);
		}

		public static SqlDataReader ExecuteReader(string connectionString, string commandText, SqlParameter[] parameters, CommandType commandType, out object[] returnValues)
		{
			return ExecuteReader(new SqlConnection(connectionString), SqlCommandBuilder(new SqlCommand(commandText), parameters), commandType, null, out returnValues);
		}

		public static SqlDataReader ExecuteReader(SqlConnection connection, SqlCommand command, string[] parameterNames, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteReader(connection, SqlCommandBuilder(command, parameterNames, parameterValues), commandType, transaction, out returnValues);
		}

		public static SqlDataReader ExecuteReader(string connectionString, SqlCommand command, string[] parameterNames, object[] parameterValues, CommandType commandType, out object[] returnValues)
		{
			return ExecuteReader(new SqlConnection(connectionString), SqlCommandBuilder(command, parameterNames, parameterValues), commandType, null, out returnValues);
		}

		public static SqlDataReader ExecuteReader(SqlConnection connection, string commandText, string[] parameterNames, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteReader(connection, SqlCommandBuilder(new SqlCommand(commandText, connection), parameterNames, parameterValues), commandType, transaction, out returnValues);
		}

		public static SqlDataReader ExecuteReader(string connectionString, string commandText, string[] parameterNames, object[] parameterValues, CommandType commandType, out object[] returnValues)
		{
			return ExecuteReader(new SqlConnection(connectionString), SqlCommandBuilder(new SqlCommand(commandText), parameterNames, parameterValues), commandType, null, out returnValues);
		}
		#endregion

		#region ExecuteScalar Overload
		public static object ExecuteScalar(SqlConnection connection, SqlCommand command, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			bool isConnectionClose = (connection.State != ConnectionState.Open);
			object o = null;
			Exception exThrown = null;
			returnValues = null;
			try
			{
				if (isConnectionClose) { connection.Open(); }
				if (command.Connection == null) { command.Connection = connection; }
				if (transaction != null) { command.Transaction = transaction; }
				command.CommandType = commandType;
				command.CommandTimeout = commandTimeOut;
				o = command.ExecuteScalar();
				returnValues = GetReturnValues(command);
			}
			catch (Exception exc)
			{
				exThrown = exc;
			}
			finally
			{
				if (connection.State != ConnectionState.Closed && isConnectionClose) {
					connection.Close();
					connection.Dispose();
				}
			}
			if (exThrown != null) { throw exThrown; }
			return o;
		}

		public static object ExecuteScalar(string connectionString, SqlCommand command, CommandType commandType, out object[] returnValues)
		{
			return ExecuteScalar(new SqlConnection(connectionString), command, commandType, null, out returnValues);
		}

		public static object ExecuteScalar(SqlConnection connection, string commandText, CommandType commandType, out object[] returnValues)
		{
			return ExecuteScalar(connection, new SqlCommand(commandText, connection), commandType, null, out returnValues);
		}

		public static object ExecuteScalar(string connectionString, string commandText, CommandType commandType, out object[] returnValues)
		{
			return ExecuteScalar(new SqlConnection(connectionString), new SqlCommand(commandText), commandType, null, out returnValues);
		}

		public static object ExecuteScalar(SqlConnection connection, SqlCommand command, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteScalar(connection, SqlCommandBuilder(command, parameterValues), commandType, transaction, out returnValues);
		}

		public static object ExecuteScalar(string connectionString, SqlCommand command, object[] parameterValues, CommandType commandType, out object[] returnValues)
		{
			return ExecuteScalar(new SqlConnection(connectionString), SqlCommandBuilder(command, parameterValues), commandType, null, out returnValues);
		}

		public static object ExecuteScalar(SqlConnection connection, SqlCommand command, SqlParameter[] parameters, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteScalar(connection, SqlCommandBuilder(command, parameters), commandType, transaction, out returnValues);
		}

		public static object ExecuteScalar(string connectionString, SqlCommand command, SqlParameter[] parameters, CommandType commandType, out object[] returnValues)
		{
			return ExecuteScalar(new SqlConnection(connectionString), SqlCommandBuilder(command, parameters), commandType, null, out returnValues);
		}

		public static object ExecuteScalar(SqlConnection connection, string commandText, SqlParameter[] parameters, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteScalar(connection, SqlCommandBuilder(new SqlCommand(commandText, connection), parameters), commandType, transaction, out returnValues);
		}

		public static object ExecuteScalar(string connectionString, string commandText, SqlParameter[] parameters, CommandType commandType, out object[] returnValues)
		{
			return ExecuteScalar(new SqlConnection(connectionString), SqlCommandBuilder(new SqlCommand(commandText), parameters), commandType, null, out returnValues);
		}

		public static object ExecuteScalar(SqlConnection connection, SqlCommand command, string[] parameterNames, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteScalar(connection, SqlCommandBuilder(command, parameterNames, parameterValues), commandType, transaction, out returnValues);
		}

		public static object ExecuteScalar(string connectionString, SqlCommand command, string[] parameterNames, object[] parameterValues, CommandType commandType, out object[] returnValues)
		{
			return ExecuteScalar(new SqlConnection(connectionString), SqlCommandBuilder(command, parameterNames, parameterValues), commandType, null, out returnValues);
		}

		public static object ExecuteScalar(SqlConnection connection, string commandText, string[] parameterNames, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteScalar(connection, SqlCommandBuilder(new SqlCommand(commandText, connection), parameterNames, parameterValues), commandType, transaction, out returnValues);
		}

		public static object ExecuteScalar(string connectionString, string commandText, string[] parameterNames, object[] parameterValues, CommandType commandType, out object[] returnValues)
		{
			return ExecuteScalar(new SqlConnection(connectionString), SqlCommandBuilder(new SqlCommand(commandText), parameterNames, parameterValues), commandType, null, out returnValues);
		}
		#endregion

		#region ExecuteXmlReader Overload
		public static XmlReader ExecuteXmlReader(SqlConnection connection, SqlCommand command, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			bool isConnectionClose = (connection.State != ConnectionState.Open);
			XmlReader r = null;
			Exception exThrown = null;
			returnValues = null;
			try
			{
				if (isConnectionClose) { connection.Open(); }
				if (command.Connection == null) { command.Connection = connection; }
				if (transaction != null) { command.Transaction = transaction; }
				command.CommandType = commandType;
				command.CommandTimeout = commandTimeOut;
				r = command.ExecuteXmlReader();
				returnValues = GetReturnValues(command);
			}
			catch (Exception exc)
			{
				exThrown = exc;
			}
			finally
			{
				if (connection.State != ConnectionState.Closed && isConnectionClose) {
					connection.Close();
					connection.Dispose();
				}
			}
			if (exThrown != null) { throw exThrown; }
			return r;
		}

		public static XmlReader ExecuteXmlReader(string connectionString, SqlCommand command, CommandType commandType, out object[] returnValues)
		{
			return ExecuteXmlReader(new SqlConnection(connectionString), command, commandType, null, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(SqlConnection connection, string commandText, CommandType commandType, out object[] returnValues)
		{
			return ExecuteXmlReader(connection, new SqlCommand(commandText, connection), commandType, null, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(string connectionString, string commandText, CommandType commandType, out object[] returnValues)
		{
			return ExecuteXmlReader(new SqlConnection(connectionString), new SqlCommand(commandText), commandType, null, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(SqlConnection connection, SqlCommand command, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteXmlReader(connection, SqlCommandBuilder(command, parameterValues), commandType, transaction, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(string connectionString, SqlCommand command, object[] parameterValues, CommandType commandType, out object[] returnValues)
		{
			return ExecuteXmlReader(new SqlConnection(connectionString), SqlCommandBuilder(command, parameterValues), commandType, null, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(SqlConnection connection, SqlCommand command, SqlParameter[] parameters, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteXmlReader(connection, SqlCommandBuilder(command, parameters), commandType, transaction, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(string connectionString, SqlCommand command, SqlParameter[] parameters, CommandType commandType, out object[] returnValues)
		{
			return ExecuteXmlReader(new SqlConnection(connectionString), SqlCommandBuilder(command, parameters), commandType, null, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(SqlConnection connection, string commandText, SqlParameter[] parameters, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteXmlReader(connection, SqlCommandBuilder(new SqlCommand(commandText, connection), parameters), commandType, transaction, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(string connectionString, string commandText, SqlParameter[] parameters, CommandType commandType, out object[] returnValues)
		{
			return ExecuteXmlReader(new SqlConnection(connectionString), SqlCommandBuilder(new SqlCommand(commandText), parameters), commandType, null, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(SqlConnection connection, SqlCommand command, string[] parameterNames, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteXmlReader(connection, SqlCommandBuilder(command, parameterNames, parameterValues), commandType, transaction, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(string connectionString, SqlCommand command, string[] parameterNames, object[] parameterValues, CommandType commandType, out object[] returnValues)
		{
			return ExecuteXmlReader(new SqlConnection(connectionString), SqlCommandBuilder(command, parameterNames, parameterValues), commandType, null, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(SqlConnection connection, string commandText, string[] parameterNames, object[] parameterValues, CommandType commandType, SqlTransaction transaction, out object[] returnValues)
		{
			return ExecuteXmlReader(connection, SqlCommandBuilder(new SqlCommand(commandText, connection), parameterNames, parameterValues), commandType, transaction, out returnValues);
		}

		public static XmlReader ExecuteXmlReader(string connectionString, string commandText, string[] parameterNames, object[] parameterValues, CommandType commandType, out object[] returnValues)
		{
			return ExecuteXmlReader(new SqlConnection(connectionString), SqlCommandBuilder(new SqlCommand(commandText), parameterNames, parameterValues), commandType, null, out returnValues);
		}
		#endregion

		private static object[] GetReturnValues(SqlCommand command)
		{
			object[] r = null;
			ArrayList a = new ArrayList();
			foreach (SqlParameter p in command.Parameters) {
				if (p.Direction != ParameterDirection.Input)
				{
					// note: includes SqlParameter.Direction = ParameterDirection.ReturnValue
					a.Add(p.Value);
				}
			}
			r = new object[a.Count];
			a.CopyTo(r);
			return r;
		}

		#region SqlCommandBuilder Overload
		public static SqlCommand SqlCommandBuilder(SqlCommand command, string[] parameterNames, object[] parameterValues)
		{
			for (int i = 0; i < parameterNames.Length; i++)
			{
				command.Parameters.AddWithValue(parameterNames.GetValue(i).ToString(), parameterValues.GetValue(i));
			}
			return command;
		}

		public static SqlCommand SqlCommandBuilder(SqlCommand command, SqlParameter[] parameters)
		{
			for (int i = 0; i < parameters.Length; i++)
			{
				command.Parameters.Add(parameters.GetValue(i));
			}
			return command;
		}

		public static SqlCommand SqlCommandBuilder(SqlCommand command, object[] parameterValues)
		{
			int j = 0;
			for (int i = 0; i < command.Parameters.Count; i++)
			{
				if (command.Parameters[i].Direction == ParameterDirection.Input || command.Parameters[i].Direction == ParameterDirection.InputOutput)
				{
					command.Parameters[i].Value = parameterValues.GetValue(j);
					j++;
				}
			}
			return command;
		}
		#endregion

		#region SqlParameterBuilder Overload
		public static SqlParameter SqlParameterBuilder(string parameterName, SqlDbType dbType, ParameterDirection direction)
		{
			SqlParameter p = new SqlParameter(parameterName, dbType);
			p.Direction = direction;
			return p;
		}

		public static SqlParameter SqlParameterBuilder(string parameterName, SqlDbType dbType, int size, ParameterDirection direction)
		{
			SqlParameter p = new SqlParameter(parameterName, dbType, size);
			p.Direction = direction;
			return p;
		}
		#endregion

		#endregion
	}
}
