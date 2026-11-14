using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace ORM
{
    public sealed class OrmContext : IOrmContext
    {
        private readonly string _connectionString;

        /// <summary>
        /// Создаёт экземпляр контекста для работы
        /// с базой данных SQL Server.
        /// </summary>
        /// <param name="connectionString">
        /// Строка подключения к базе данных.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Строка подключения не задана.
        /// </exception>
        public OrmContext(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(
                connectionString);

            _connectionString = connectionString;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<T>> GetAllAsync<T>(
            string tableName,
            CancellationToken cancellationToken = default)
            where T : class, new()
        {
            string safeTableName = EscapeIdentifier(tableName);

            string sql = $"SELECT * FROM {safeTableName}";

            var result = new List<T>();

            await using var connection =
                new SqlConnection(_connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command =
                new SqlCommand(sql, connection);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            PropertyInfo[] properties = typeof(T)
                .GetProperties(BindingFlags.Public |
                               BindingFlags.Instance)
                .Where(property => property.CanWrite &&
                                   property.SetMethod?.IsPublic == true)
                .ToArray();

            var columns = new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

            for (int index = 0; index < reader.FieldCount; index++)
            {
                columns[reader.GetName(index)] = index;
            }

            while (await reader.ReadAsync(cancellationToken))
            {
                T entity = new();

                foreach (PropertyInfo property in properties)
                {
                    if (!columns.TryGetValue(
                            property.Name, out int ordinal))
                    {
                        continue;
                    }

                    if (await reader.IsDBNullAsync(
                            ordinal, cancellationToken))
                    {
                        if (!property.PropertyType.IsValueType ||
                            Nullable.GetUnderlyingType(
                                property.PropertyType) != null)
                        {
                            property.SetValue(entity, null);
                        }

                        continue;
                    }

                    object value = reader.GetValue(ordinal);

                    Type targetType =
                        Nullable.GetUnderlyingType(
                            property.PropertyType)
                        ?? property.PropertyType;

                    object convertedValue =
                        targetType.IsInstanceOfType(value)
                            ? value
                            : targetType.IsEnum
                                ? Enum.ToObject(targetType, value)
                                : Convert.ChangeType(
                                    value,
                                    targetType,
                                    CultureInfo.InvariantCulture);

                    property.SetValue(entity, convertedValue);
                }

                result.Add(entity);
            }

            return result;
        }

        /// <summary>
        /// Проверяет имя SQL-идентификатора и экранирует его.
        /// </summary>
        /// <param name="identifier">
        /// Имя таблицы или столбца.
        /// </param>
        /// <returns>
        /// Проверенный SQL-идентификатор в квадратных скобках.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Имя идентификатора имеет недопустимый формат.
        /// </exception>
        private static string EscapeIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier) ||
                !Regex.IsMatch(
                    identifier,
                    @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                throw new ArgumentException(
                    "Недопустимое имя SQL-идентификатора.",
                    nameof(identifier));
            }

            return $"[{identifier}]";
        }

        /// <inheritdoc />
        public async Task<int> InsertAsync<T>(
            string tableName,
            T entity,
            CancellationToken cancellationToken = default)
            where T : class
        {
            string safeTableName = EscapeIdentifier(tableName);
            PropertyInfo[] properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.Name.ToLower() != "id")
            .ToArray();

            string columns = string.Join(", ", properties.Select(p => EscapeIdentifier(p.Name)));
            string parameters = string.Join(", ", properties.Select(p => "@" + p.Name));
            string sql = $@"insert into {safeTableName} ({columns})
                    values ({parameters})";
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(entity);
                command.Parameters.AddWithValue("@" + property.Name, value ?? DBNull.Value);
            }
            await connection.OpenAsync(cancellationToken);
            object result = await command.ExecuteScalarAsync(cancellationToken);

            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }

            return 0;
        }

        /// <inheritdoc />
        public async Task<int> DeleteAsync(
            string tableName,
            int id,
            CancellationToken cancellationToken = default)
        {
            string safeTableName = EscapeIdentifier(tableName);
            string sql = $"delete from {safeTableName} where [Id] = @id";
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            await connection.OpenAsync(cancellationToken);
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected;
        }

        /// <inheritdoc />
        public async Task<int> UpdateAsync<T>(
            string tableName,
            int id,
            T entity,
            CancellationToken cancellationToken = default)
            where T : class
        {
            string safeTableName = EscapeIdentifier(tableName);
            PropertyInfo[] properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.Name.ToLower() != "id")
            .ToArray();
            string[] columns = string.Join(", ", properties.Select(p => EscapeIdentifier(p.Name))).Split(", ");
            string[] parameters = string.Join(", ", properties.Select(p => "@" + p.Name)).Split(", ");
            string sql = $@"update {safeTableName}
                        set ";
            StringBuilder sb = new StringBuilder();
            sb.Append(sql);
            for (int i = 0; i < columns.Length; i++)
            {
                sb.Append($"{columns[i]} = {parameters[i]}");
                if (i < columns.Length - 1) sb.Append(", ");
            }
            sb.Append($" where [Id] = @id");
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sb.ToString(), connection);
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(entity);
                command.Parameters.AddWithValue("@" + property.Name, value ?? DBNull.Value);
            }
            command.Parameters.AddWithValue("@id", id);
            await connection.OpenAsync(cancellationToken);
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected;
        }
    }
}
