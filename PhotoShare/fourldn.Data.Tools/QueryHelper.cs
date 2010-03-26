using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Configuration;

namespace fourldn.Data.Tools
{
	internal class QueryHelper : IDisposable
	{
		DbProviderFactory m_fact;
		string            m_cstr;
		DbConnection      m_conn;
		DbTransaction     m_tran;

		public QueryHelper(string conn_name)
		{
			var cstr = ConfigurationManager.ConnectionStrings[conn_name];

			if( cstr == null )
				throw new ArgumentException("The given connection string does not exist.", "conn_name");

			m_cstr = cstr.ConnectionString;
			m_fact = DbProviderFactories.GetFactory(cstr.ProviderName);
		}

		public QueryHelper(string prov_name, string conn_str)
		{
			m_cstr = conn_str;
			m_fact = DbProviderFactories.GetFactory(prov_name);
		}

		public bool TransactionActive
		{
			get { return m_tran != null; }
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if( disposing )
			{
				if( m_tran != null )
				{
					m_tran.Dispose();
					m_tran = null;
				}

				if( m_conn != null )
				{
					m_conn.Dispose();
					m_conn = null;
				}
			}
		}

		public void BeginTransaction()
		{
			BeginTransaction(IsolationLevel.Unspecified);
		}

		public void BeginTransaction(IsolationLevel isol)
		{
			if( m_conn != null || m_tran != null )
				throw new InvalidOperationException("A transaction is already in progress.");

			m_conn = m_fact.CreateConnection();

			m_conn.ConnectionString = m_cstr;

			m_conn.Open();

			m_tran = m_conn.BeginTransaction(isol);
		}

		public void CommitTransaction()
		{
			if( m_conn == null || m_tran == null )
				throw new InvalidOperationException("A transaction is not currently in progress.");

			try
			{
				m_tran.Commit();

				m_conn.Close();
			}
			finally
			{
				try
				{
					m_tran.Dispose();
					m_conn.Dispose();
				}
				catch
				{
				}

				m_tran = null;
				m_conn = null;
			}
		}

		public void RollbackTransaction()
		{
			if( m_conn == null || m_tran == null )
				throw new InvalidOperationException("A transaction is not currently in progress.");

			try
			{
				m_tran.Rollback();

				m_conn.Close();
			}
			finally
			{
				try
				{
					m_tran.Dispose();
					m_conn.Dispose();
				}
				catch
				{
				}

				m_tran = null;
				m_conn = null;
			}
		}

		public DbParameter CreateParameter()
		{
			return m_fact.CreateParameter();
		}

		public DbParameter CreateParameter(string name)
		{
			var parm = m_fact.CreateParameter();

			parm.ParameterName = name;
			
			return parm;
		}

		public DbParameter CreateParameter(string name, DbType type)
		{
			var parm = m_fact.CreateParameter();

			parm.ParameterName = name;
			parm.DbType        = type;

			return parm;
		}

		public DbParameter CreateParameter(string name, DbType type, int size)
		{
			var parm = m_fact.CreateParameter();

			parm.ParameterName = name;
			parm.DbType        = type;
			parm.Size          = size;

			return parm;
		}

		public DbParameter CreateParameter(string name, DbType type, object value)
		{
			var parm = m_fact.CreateParameter();

			parm.ParameterName = name;
			parm.DbType        = type;
			parm.Value         = value;

			return parm;
		}

		public DbParameter CreateParameter(string name, DbType type, int size, object value)
		{
			var parm = m_fact.CreateParameter();

			parm.ParameterName = name;
			parm.DbType        = type;
			parm.Size          = size;
			parm.Value         = value;

			return parm;
		}

		public void ExecuteProcedure(string name, IEnumerable<DbParameter> parms_in, Action<IEnumerable<DbParameter>> results_act)
		{
			ExecuteCommand(name, CommandType.StoredProcedure, parms_in, results_act);
		}

		public void ExecuteProcedure(string name, IEnumerable<DbParameter> parms_in, Action<IEnumerable<DbParameter>, DbDataReader> results_act)
		{
			ExecuteCommand(name, CommandType.StoredProcedure, parms_in, results_act);
		}

		public void ExecuteQuery(string text, IEnumerable<DbParameter> parms_in)
		{
			ExecuteCommand(text, CommandType.Text, parms_in, (Action<IEnumerable<DbParameter>>)null);
		}

		public void ExecuteQuery(string text, IEnumerable<DbParameter> parms_in, Action<IEnumerable<DbParameter>> results_act)
		{
			ExecuteCommand(text, CommandType.Text, parms_in, results_act);
		}

		public void ExecuteQuery(string text, IEnumerable<DbParameter> parms_in, Action<IEnumerable<DbParameter>, DbDataReader> results_act)
		{
			ExecuteCommand(text, CommandType.Text, parms_in, results_act);
		}

		private void ExecuteCommand(string query, CommandType query_type, IEnumerable<DbParameter> parms_in, Action<IEnumerable<DbParameter>> results_act)
		{
			if( m_conn != null )
			{
				using( var cmd = m_conn.CreateCommand() )
					ExecuteCommand(query, query_type, cmd, parms_in, results_act);
			}
			else
			{
				using( var conn = m_fact.CreateConnection() )
				{
					conn.ConnectionString = m_cstr;

					conn.Open();

					using( var cmd = conn.CreateCommand() )
						ExecuteCommand(query, query_type, cmd, parms_in, results_act);

					conn.Close();
				}
			}
		}

		private void ExecuteCommand(string query, CommandType query_type, IEnumerable<DbParameter> parms_in, Action<IEnumerable<DbParameter>, DbDataReader> results_act)
		{
			if( m_conn != null )
			{
				using( var cmd = m_conn.CreateCommand() )
					ExecuteCommand(query, query_type, cmd, parms_in, results_act);
			}
			else
			{
				using( var conn = m_fact.CreateConnection() )
				{
					conn.ConnectionString = m_cstr;

					conn.Open();

					using( var cmd = conn.CreateCommand() )
						ExecuteCommand(query, query_type, cmd, parms_in, results_act);

					conn.Close();
				}
			}
		}

		private void ExecuteCommand(string query, CommandType query_type, DbCommand cmd, IEnumerable<DbParameter> parms_in, Action<IEnumerable<DbParameter>> results_act) // ExecuteNonQuery
		{
			cmd.CommandType = query_type;
			cmd.CommandText = query;

			if( m_tran != null )
				cmd.Transaction = m_tran;

			if( parms_in != null )
			{
				foreach( var parm in parms_in )
					cmd.Parameters.Add(parm);
			}

			cmd.ExecuteNonQuery();

			if( results_act != null )
				results_act(cmd.Parameters.Cast<DbParameter>().Where(p => p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue));
		}

		private void ExecuteCommand(string query, CommandType query_type, DbCommand cmd, IEnumerable<DbParameter> parms_in, Action<IEnumerable<DbParameter>, DbDataReader> results_act) // ExecuteReader
		{
			cmd.CommandType = query_type;
			cmd.CommandText = query;

			if( m_tran != null )
				cmd.Transaction = m_tran;

			if( parms_in != null )
			{
				foreach( var parm in parms_in )
					cmd.Parameters.Add(parm);
			}

			using( var dr = cmd.ExecuteReader() )
			{
				if( results_act != null )
					results_act(cmd.Parameters.Cast<DbParameter>().Where(p => p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue), dr);
			}
		}
		
		public T ExecuteProcedure<T>(string name, IEnumerable<DbParameter> parms_in, Func<IEnumerable<DbParameter>, T> results_act)
		{
			return ExecuteCommand(name, CommandType.StoredProcedure, parms_in, results_act);
		}

		public T ExecuteProcedure<T>(string name, IEnumerable<DbParameter> parms_in, Func<IEnumerable<DbParameter>, DbDataReader, T> results_act)
		{
			return ExecuteCommand(name, CommandType.StoredProcedure, parms_in, results_act);
		}

		public T ExecuteQuery<T>(string text, IEnumerable<DbParameter> parms_in, Func<IEnumerable<DbParameter>, T> results_act)
		{
			return ExecuteCommand(text, CommandType.Text, parms_in, results_act);
		}

		public T ExecuteQuery<T>(string text, IEnumerable<DbParameter> parms_in, Func<IEnumerable<DbParameter>, DbDataReader, T> results_act)
		{
			return ExecuteCommand(text, CommandType.Text, parms_in, results_act);
		}

		private T ExecuteCommand<T>(string query, CommandType query_type, IEnumerable<DbParameter> parms_in, Func<IEnumerable<DbParameter>, T> results_act)
		{
			if( m_conn != null )
			{
				using( var cmd = m_conn.CreateCommand() )
					return ExecuteCommand(query, query_type, cmd, parms_in, results_act);
			}
			else
			{
				T ret = default(T);
				using( var conn = m_fact.CreateConnection() )
				{
					conn.ConnectionString = m_cstr;

					conn.Open();

					using( var cmd = conn.CreateCommand() )
						ret = ExecuteCommand(query, query_type, cmd, parms_in, results_act);

					conn.Close();
				}
				return ret;
			}
		}

		private T ExecuteCommand<T>(string query, CommandType query_type, IEnumerable<DbParameter> parms_in, Func<IEnumerable<DbParameter>, DbDataReader, T> results_act)
		{
			if( m_conn != null )
			{
				using( var cmd = m_conn.CreateCommand() )
					return ExecuteCommand(query, query_type, cmd, parms_in, results_act);
			}
			else
			{
				T ret = default(T);
				using( var conn = m_fact.CreateConnection() )
				{
					conn.ConnectionString = m_cstr;

					conn.Open();

					using( var cmd = conn.CreateCommand() )
						ret = ExecuteCommand(query, query_type, cmd, parms_in, results_act);

					conn.Close();
				}
				return ret;
			}
		}

		private T ExecuteCommand<T>(string query, CommandType query_type, DbCommand cmd, IEnumerable<DbParameter> parms_in, Func<IEnumerable<DbParameter>, T> results_act) // ExecuteNonQuery
		{
			cmd.CommandType = query_type;
			cmd.CommandText = query;

			if( m_tran != null )
				cmd.Transaction = m_tran;

			if( parms_in != null )
			{
				foreach( var parm in parms_in )
					cmd.Parameters.Add(parm);
			}

			T ret = default(T);
			cmd.ExecuteNonQuery();

			if( results_act != null )
				ret = results_act(cmd.Parameters.Cast<DbParameter>().Where(p => p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue));

			return ret;
		}

		private T ExecuteCommand<T>(string query, CommandType query_type, DbCommand cmd, IEnumerable<DbParameter> parms_in, Func<IEnumerable<DbParameter>, DbDataReader, T> results_act) // ExecuteReader
		{
			cmd.CommandType = query_type;
			cmd.CommandText = query;

			if( m_tran != null )
				cmd.Transaction = m_tran;

			if( parms_in != null )
			{
				foreach( var parm in parms_in )
					cmd.Parameters.Add(parm);
			}

			T ret = default(T);
			using( var dr = cmd.ExecuteReader() )
			{
				if( results_act != null )
					ret = results_act(cmd.Parameters.Cast<DbParameter>().Where(p => p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue), dr);
			}
			return ret;
		}
	}
}