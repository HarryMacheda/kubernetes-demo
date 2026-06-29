using Npgsql;

public class PostgresQuizRepository : IQuizRepository
{
    private readonly string _connectionString;

    public PostgresQuizRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<IQuiz> CreateQuizAsync(IQuiz quiz)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO Quizs (Name, Type, CreatedAt, CreatedBy)
                    VALUES (@Name, @Type, @CreatedAt, @CreatedBy)
                    RETURNING Id;
                    ";

                command.Parameters.AddWithValue("@Name", quiz.Name);
                command.Parameters.AddWithValue("@Type", quiz.Type);
                command.Parameters.AddWithValue("@CreatedAt", quiz.CreatedAt);
                command.Parameters.AddWithValue("@CreatedBy", quiz.CreatedBy);

                var result = await command.ExecuteScalarAsync();
                quiz.Id = Convert.ToInt32(result);
            }
        }

        return quiz;
    }

    public async Task<IQuiz?> GetQuizByIdAsync(int id)
    {
        IQuiz quiz = null;
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT Name, Type, CreatedAt, CreatedBy
                    FROM Quizs
                    WHERE Id = @Id;";

                command.Parameters.AddWithValue("@Id", id);

                var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    quiz = BaseQuiz.QuizFromType(
                        id,
                        reader.GetString(reader.GetOrdinal("Name")),
                        (QuizType)reader.GetInt32(reader.GetOrdinal("Type")),
                        reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        reader.GetGuid(reader.GetOrdinal("CreatedBy"))
                    );
                }
            }
        }

        return quiz;
    }
                        
    public async Task<IEnumerable<IQuiz>> GetAllQuizzesAsync()
    {
        var quizzes = new List<IQuiz>();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT Id, Name, Type, CreatedAt, CreatedBy
                    FROM Quizs";

                var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    quizzes.Add(BaseQuiz.QuizFromType(
                        reader.GetInt32(reader.GetOrdinal("Id")),
                        reader.GetString(reader.GetOrdinal("Name")),
                        (QuizType)reader.GetInt32(reader.GetOrdinal("Type")),
                        reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        reader.GetGuid(reader.GetOrdinal("CreatedBy"))
                    ));
                }
            }
        }

        return quizzes;
    }

    public async Task UpdateQuizAsync(IQuiz quiz)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE Quizs
                    SET Name = @Name, Type = @Type, CreatedAt = @CreatedAt, CreatedBy = @CreatedBy
                    WHERE Id = @Id";

                var reader = await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task DeleteQuizAsync(int id)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    DELETE Quizs
                    WHERE Id = @Id";

                var reader = await command.ExecuteNonQueryAsync();
            }
        }
    }
}