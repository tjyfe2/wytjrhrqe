﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CodeAnalysis.SQLite.Interop;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.SQLite.v2.Interop
{
    /// <summary>
    /// Represents a prepared sqlite statement.  <see cref="SqlStatement"/>s can be 
    /// <see cref="Step"/>ed (i.e. executed).  Executing a statement can result in 
    /// either <see cref="Result.DONE"/> if the command completed and produced no
    /// value, or <see cref="Result.ROW"/> if it evaluated out to a sql row that can
    /// then be queried.
    /// <para/>
    /// If a statement is parameterized then parameters can be provided by the 
    /// BindXXX overloads.  Bind is 1-based (to match sqlite).  
    /// <para/>
    /// When done executing a statement, the statement should be <see cref="Reset"/>.
    /// The easiest way to ensure this is to just use a 'using' statement along with
    /// a <see cref="ResettableSqlStatement"/>.  By resetting the statement, it can
    /// then be used in the future with new bound parameters.
    /// <para/>
    /// Finalization/destruction of the underlying raw sqlite statement is handled
    /// by <see cref="SqlConnection.Close_OnlyForUseBySQLiteConnectionPool"/>.
    /// </summary>
    internal readonly struct SqlStatement
    {
        private readonly SqlConnection _connection;
        private readonly SafeSqliteStatementHandle _rawStatement;

        public SqlStatement(SqlConnection connection, SafeSqliteStatementHandle statement)
        {
            _connection = connection;
            _rawStatement = statement;
        }

        internal void Close_OnlyForUseBySqlConnection()
            => _rawStatement.Dispose();

        public void ClearBindings()
            => _connection.ThrowIfNotOk(NativeMethods.sqlite3_clear_bindings(_rawStatement));

        public void Reset()
            => _connection.ThrowIfNotOk(NativeMethods.sqlite3_reset(_rawStatement));

        public Result Step(bool throwOnError = true)
        {
            var stepResult = NativeMethods.sqlite3_step(_rawStatement);

            // Anything other than DONE or ROW is an error when stepping.
            // throw if the caller wants that, or just return the value
            // otherwise.
            if (stepResult != Result.DONE && stepResult != Result.ROW)
            {
                if (throwOnError)
                {
                    _connection.Throw(stepResult);
                    throw ExceptionUtilities.Unreachable;
                }
            }

            return stepResult;
        }

        internal void BindStringParameter(int parameterIndex, string value)
            => _connection.ThrowIfNotOk(NativeMethods.sqlite3_bind_text(_rawStatement, parameterIndex, value));

        internal void BindInt64Parameter(int parameterIndex, long value)
            => _connection.ThrowIfNotOk(NativeMethods.sqlite3_bind_int64(_rawStatement, parameterIndex, value));

        internal void BindBlobParameter(int parameterIndex, ReadOnlySpan<byte> bytes)
            => _connection.ThrowIfNotOk(NativeMethods.sqlite3_bind_blob(_rawStatement, parameterIndex, bytes));

        internal int GetInt32At(int columnIndex)
            => NativeMethods.sqlite3_column_int(_rawStatement, columnIndex);

        internal long GetInt64At(int columnIndex)
            => NativeMethods.sqlite3_column_int64(_rawStatement, columnIndex);

        internal string GetStringAt(int columnIndex)
            => NativeMethods.sqlite3_column_text(_rawStatement, columnIndex);
    }
}
