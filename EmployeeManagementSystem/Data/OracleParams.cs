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
        private sealed class Entry
        {
            public string Name;
            public object Value;
            public OracleDbType? Type;      // null means "let ODP.NET work it out"
        }

        private readonly List<Entry> _values = new List<Entry>();

        public static OracleParams New()
        {
            return new OracleParams();
        }

        /// <summary>Binds ":name" to <paramref name="value"/> (null becomes DBNull).</summary>
        public OracleParams Set(string name, object value)
        {
            return Add(name, value, null);
        }

        /// <summary>
        /// Binds a BLOB, including a null one.
        ///
        /// The type has to be stated. Left to infer, ODP.NET picks Raw for a byte
        /// array, and Raw stops at 2000 bytes - so a photo of any real size fails with
        /// ORA-01460 instead of being stored. A null is worse still: DBNull carries no
        /// type at all, so the driver has nothing to infer from.
        /// </summary>
        public OracleParams SetBlob(string name, byte[] value)
        {
            return Add(name, value, OracleDbType.Blob);
        }

        private OracleParams Add(string name, object value, OracleDbType? type)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Parameter name is required.", "name");
            }

            _values.Add(new Entry
            {
                Name = name.TrimStart(':'),
                Value = value ?? DBNull.Value,
                Type = type,
            });

            return this;
        }

        void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            var oracleCommand = command as OracleCommand;
            if (oracleCommand != null)
            {
                oracleCommand.BindByName = true;
            }

            foreach (Entry entry in _values)
            {
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = entry.Name;
                parameter.Value = entry.Value;

                var oracleParameter = parameter as OracleParameter;
                if (oracleParameter != null && entry.Type.HasValue)
                {
                    oracleParameter.OracleDbType = entry.Type.Value;
                }

                command.Parameters.Add(parameter);
            }
        }
    }
}
