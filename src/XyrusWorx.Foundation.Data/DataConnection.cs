using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using JetBrains.Annotations;
using XyrusWorx.IO;

namespace XyrusWorx.Data
{
	[PublicAPI]
	public class DataConnection : Resource
	{
		private readonly IDbConnection mConnection;
		private readonly Stack<DataTransaction> mTransaction;

		internal DataConnection([NotNull] IDbConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException(nameof(connection));
			}

			mConnection = connection;
			mTransaction = new Stack<DataTransaction>();
		}

		public IScope Open()
		{
			var scope = new Scope(() => mConnection.Open(), () => mConnection.Close());

			return scope.Enter();
		}
		public DataTransaction BeginTransaction(DataTransactionIsolation isolation = DataTransactionIsolation.ReadCommitted)
		{
			DataTransaction transaction = null;

			// ReSharper disable once AccessToModifiedClosure
			transaction = new DataTransaction(mConnection, () => PopTransaction(transaction), isolation);
			mTransaction.Push(transaction);

			return transaction;
		}

		public bool IsConnected => mConnection.State == ConnectionState.Open;

		public T Get<T>([NotNull] string sql, IKeyValueStore parameters = null)
		{
			if (sql.NormalizeNull() == null) throw new ArgumentNullException(nameof(sql));

			using (var command = CreateCommand(sql))
			{
				SetParameters(command, parameters);

				var obj = command.ExecuteScalar();
				if (!(obj is T))
				{
					return default(T);
				}

				return (T)obj;
			}
		}
		public int Execute([NotNull] string sql, IKeyValueStore parameters = null, int timeout = 30)
		{
			if (sql.NormalizeNull() == null) throw new ArgumentNullException(nameof(sql));

			using (var command = CreateCommand(sql))
			{
				command.CommandTimeout = timeout;
				SetParameters(command, parameters);
				return command.ExecuteNonQuery();
			}
		}

		public async Task<T> GetAsync<T>([NotNull] string sql, IKeyValueStore parameters = null)
		{
			if (sql.NormalizeNull() == null) throw new ArgumentNullException(nameof(sql));

			using (var command = CreateCommand(sql))
			{
				SetParameters(command, parameters);

				var obj = await command.ExecuteScalarAsync();
				if (!(obj is T))
				{
					return default(T);
				}

				return (T)obj;
			}
		}
		public async Task<int> ExecuteAsync([NotNull] string sql, IKeyValueStore parameters = null, int timeout = 30)
		{
			if (sql.NormalizeNull() == null) throw new ArgumentNullException(nameof(sql));

			using (var command = CreateCommand(sql))
			{
				command.CommandTimeout = timeout;
				SetParameters(command, parameters);
				return await command.ExecuteNonQueryAsync();
			}
		}

		public DataReader CreateReader([NotNull] string sql, IKeyValueStore parameters = null)
		{
			if (sql.NormalizeNull() == null) throw new ArgumentNullException(nameof(sql));
			
			using (var command = CreateCommand(sql))
			{
				SetParameters(command, parameters);

				return new DataReader(command.ExecuteReader());
			}
		}
		public async Task<DataReader> CreateReaderAsync([NotNull] string sql, IKeyValueStore parameters = null)
		{
			if (sql.NormalizeNull() == null) throw new ArgumentNullException(nameof(sql));

			using (var command = CreateCommand(sql))
			{
				SetParameters(command, parameters);

				return new DataReader(await command.ExecuteReaderAsync());
			}
		}

		protected sealed override void DisposeOverride()
		{
			if (IsConnected)
			{
				mConnection.Close();
			}
		}

		private DbCommand CreateCommand(string sql)
		{
			var command = mConnection.CreateCommand();
			var transaction = (IDbTransaction) null;

			if (mTransaction.Count > 0)
			{
				transaction = mTransaction.Peek().GetTransaction();
			}

			command.Transaction = transaction;
			command.CommandText = sql;
			command.CommandType = CommandType.Text;

			return (DbCommand)command;
		}
		private void SetParameters(IDbCommand command, IKeyValueStore parameters)
		{
			if (parameters == null)
			{
				return;
			}

			foreach (var key in parameters.GetKeys())
			{
				var dbParameter = command.CreateParameter();

				dbParameter.ParameterName = key;
				dbParameter.Value = parameters.Read(key);

				command.Parameters.Add(dbParameter);
			}
		}
		private void PopTransaction(DataTransaction transaction)
		{
			if (mTransaction.Count == 0)
			{
				return;
			}

			var current = mTransaction.Pop();
			if (current == null)
			{
				return;
			}

			if (!ReferenceEquals(current, transaction))
			{
				throw new InvalidOperationException("Invalid transaction exit order. When cascading transactions, the inner transactions have to be closed before the outer transactions.");
			}
		}
	}
}
