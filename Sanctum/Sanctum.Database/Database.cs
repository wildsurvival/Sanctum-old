/*
Sanctum is a free open-source 2D isometric game engine
Copyright (C) 2013  Andrew Choate

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

You can contact the author at a_choate@live.com or at the project website
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data.OracleClient;

namespace Sanctum.Database
{
    public enum DatabaseType
    {
        MySql = 0,
        Sqlite = 1
    }

    #region "Result Classes"
    public class Field
    {
        public string Name;
        public object Value;

        public Field(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class Row
    {
        public OrderedDictionary Fields = new OrderedDictionary();

        public Field this[string index]
        {
            get { return (Field)Fields[index]; }
        }

        public Field this[int index]
        {
            get { return (Field)Fields[index]; }
        }
    }

    public class QueryResult
    {
        public List<Row> Rows;

        public int RowsAffected;

        public object this[int index]
        {
            get { return Rows[index]; }
        }

        public QueryResult()
        {
            Rows = new List<Row>();
        }
    }
    #endregion

    public class Database
    {
        private OracleConnection handle;

        public Database(string username, string password, string host, int port, string database)
        {
            string connection = string.Format("User Id={0};Password={1};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={2})(PORT={3}))(CONNECT_DATA=(SID={4})));",
                    username,
                    password,
                    host,
                    port,
                    database
                );

            handle.ConnectionString = connection;
        }

        public void Open()
        {
            handle.Open();
        }

        public QueryResult Query(string query)
        {
            if (handle.State != System.Data.ConnectionState.Open)
                return null;

            OracleCommand command = handle.CreateCommand();
            command.CommandText = query;

            OracleDataReader reader = command.ExecuteReader();

            QueryResult result = new QueryResult();

            while (reader.Read())
            {
                Row row = new Row();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Fields[reader.GetName(i)] = new Field(reader.GetName(i), reader[i]);
                }

                result.Rows.Add(row);
            }

            reader.Close();

            return result;
        }

        public int NonQuery(string query)
        {
            if (handle.State != System.Data.ConnectionState.Open)
                return 0;

            OracleCommand command = handle.CreateCommand();
            command.CommandText = query;

            return command.ExecuteNonQuery();
        }

        public void Close()
        {
            handle.Close();
        }
    }
}
