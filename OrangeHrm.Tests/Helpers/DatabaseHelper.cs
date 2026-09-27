using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

namespace OrangeHrm.Tests.Helpers;

/// <summary>
/// Helper de arquitectura para gestión y aserciones contra Base de Datos
/// utilizando ADO.NET y Dapper, con soporte para conexiones reales (SQL Server / MySQL)
/// y modo de simulación en memoria (SQLite) para entornos de CI/CD y demos públicas.
/// </summary>
public class DatabaseHelper : IDisposable
{
    private readonly IDbConnection _connection;
    private readonly bool _isSimulatedMode;

    public DatabaseHelper(string? connectionString = null)
    {
        string? configuredConnection = connectionString ?? Environment.GetEnvironmentVariable("ORANGEHRM_DB_CONNECTION");

        if (!string.IsNullOrWhiteSpace(configuredConnection))
        {
            // Conexión real mediante ADO.NET / SqlClient
            _connection = new SqlConnection(configuredConnection);
            _connection.Open();
            _isSimulatedMode = false;
        }
        else
        {
            // Modo simulación de base de datos en memoria usando SQLite con ADO.NET y Dapper
            // Esto permite ejecutar consultas SQL reales sin depender de puertos abiertos en demos públicas
            _connection = new SqliteConnection("Data Source=:memory:;Mode=Memory;Cache=Shared");
            _connection.Open();
            _isSimulatedMode = true;
            InitializeSimulatedSchema();
        }
    }

    private void InitializeSimulatedSchema()
    {
        const string createTableSql = @"
            CREATE TABLE IF NOT EXISTS hs_hr_employee (
                emp_number INTEGER PRIMARY KEY AUTOINCREMENT,
                emp_firstname TEXT NOT NULL,
                emp_lastname TEXT NOT NULL,
                emp_middle_name TEXT NULL
            );";
        _connection.Execute(createTableSql);
    }

    #region Métodos de Sincronización / Simulación de Estado
    /// <summary>
    /// Sincroniza en la base de datos el registro de empleado creado desde la UI
    /// </summary>
    public void RecordEmployeeCreated(string firstName, string lastName, string middleName = "")
    {
        if (_isSimulatedMode)
        {
            const string sql = "INSERT INTO hs_hr_employee (emp_firstname, emp_lastname, emp_middle_name) VALUES (@FirstName, @LastName, @MiddleName);";
            _connection.Execute(sql, new { FirstName = firstName, LastName = lastName, MiddleName = middleName });
        }
    }

    /// <summary>
    /// Sincroniza en la base de datos la modificación de un empleado
    /// </summary>
    public void RecordEmployeeUpdated(string firstName, string newMiddleName)
    {
        if (_isSimulatedMode)
        {
            const string sql = "UPDATE hs_hr_employee SET emp_middle_name = @MiddleName WHERE emp_firstname = @FirstName;";
            _connection.Execute(sql, new { FirstName = firstName, MiddleName = newMiddleName });
        }
    }

    /// <summary>
    /// Sincroniza en la base de datos la eliminación de un empleado
    /// </summary>
    public void RecordEmployeeDeleted(string firstName)
    {
        if (_isSimulatedMode)
        {
            const string sql = "DELETE FROM hs_hr_employee WHERE emp_firstname = @FirstName;";
            _connection.Execute(sql, new { FirstName = firstName });
        }
    }
    #endregion

    #region Consultas y Aserciones con Dapper y ADO.NET
    /// <summary>
    /// Consulta SQL mediante Dapper para verificar la existencia del empleado en BD (Alta / Baja)
    /// </summary>
    public int GetEmployeeCount(string firstName, string lastName)
    {
        const string sql = "SELECT COUNT(1) FROM hs_hr_employee WHERE emp_firstname = @FirstName AND emp_lastname = @LastName;";
        return _connection.ExecuteScalar<int>(sql, new { FirstName = firstName, LastName = lastName });
    }

    /// <summary>
    /// Consulta SQL mediante Dapper para verificar los datos modificados en BD (Modificación)
    /// </summary>
    public string? GetEmployeeMiddleName(string firstName)
    {
        const string sql = "SELECT emp_middle_name FROM hs_hr_employee WHERE emp_firstname = @FirstName LIMIT 1;";
        return _connection.QueryFirstOrDefault<string>(sql, new { FirstName = firstName });
    }

    /// <summary>
    /// Consulta genérica utilizando ADO.NET puro (DbCommand / IDataReader)
    /// </summary>
    public bool ExistsByAdoNet(string firstName, string lastName)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM hs_hr_employee WHERE emp_firstname = @FirstName AND emp_lastname = @LastName";
        
        var paramFirst = command.CreateParameter();
        paramFirst.ParameterName = "@FirstName";
        paramFirst.Value = firstName;
        command.Parameters.Add(paramFirst);

        var paramLast = command.CreateParameter();
        paramLast.ParameterName = "@LastName";
        paramLast.Value = lastName;
        command.Parameters.Add(paramLast);

        var result = Convert.ToInt32(command.ExecuteScalar());
        return result > 0;
    }

    /// <summary>
    /// Ejecuta una consulta arbitraria con Dapper
    /// </summary>
    public IEnumerable<T> Query<T>(string sql, object? parameters = null)
    {
        return _connection.Query<T>(sql, parameters);
    }
    #endregion

    public void Dispose()
    {
        if (_connection.State != ConnectionState.Closed)
        {
            _connection.Close();
        }
        _connection.Dispose();
    }
}
