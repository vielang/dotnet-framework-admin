using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace EmployeeManagementSystem.Data
{
    /// <summary>
    /// Dapper parameter bag for ODP.NET.
    ///
    /// It exists for one reason: ODP.NET binds parameters *by position* unless
    /// <see cref="OracleCommand.BindByName"/> is set, and Dapper gives no other hook
    /// to set that flag. Passing a plain anonymous object would therefore bind
    /// silently by declaration order - correct until someone reorders a property.
    /// </summary>
    public sealed class OracleParams : SqlMapper.IDynamicParameters
    {
        private readonly List<KeyValuePair<string, object>> _values =
            new List<KeyValuePair<string, object>>();

        public static OracleParams New()
        {
            return new OracleParams();
        }

        /// <summary>Binds ":name" to <paramref name="value"/> (null becomes DBNull).</summary>
        public OracleParams Set(string name, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Parameter name is required.", "name");
            }

            _values.Add(new KeyValuePair<string, object>(name.TrimStart(':'), value ?? DBNull.Value));
            return this;
        }

        void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            var oracleCommand = command as OracleCommand;
            if (oracleCommand != null)
            {
                oracleCommand.BindByName = true;
            }

            foreach (KeyValuePair<string, object> value in _values)
            {
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = value.Key;
                parameter.Value = value.Value;
                command.Parameters.Add(parameter);
            }
        }
    }
}
