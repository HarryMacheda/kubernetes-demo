using Npgsql;

public class PostgresQuestionOptionsRepository : IQuestionOptionsRepository
{
    private readonly string _connectionString;

    public PostgresQuestionOptionsRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    
    public async Task<IQuestionOption> CreateQuestionOptionAsync(IQuestionOption questionOption)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO QuestionOptions (QuestionId, Index, IsCorrect, OptionText, OptionAnswerText)
                    VALUES (@QuestionId, @Index, @IsCorrect, @OptionText, @OptionAnswerText)
                    RETURNING Id;
                    ";

                command.Parameters.AddWithValue("@QuestionId", questionOption.QuestionId);
                command.Parameters.AddWithValue("@Index", questionOption.Index);
                command.Parameters.AddWithValue("@IsCorrect", questionOption.IsCorrect);
                command.Parameters.AddWithValue("@OptionText", questionOption.OptionText);
                command.Parameters.AddWithValue("@OptionAnswerText", questionOption.OptionAnswerText);

                var result = await command.ExecuteScalarAsync();
                questionOption.Id = Convert.ToInt32(result);
            }
        }

        return questionOption;
    }

    public async Task<IQuestionOption?> GetQuestionOptionByIdAsync(int id)
    {
        IQuestionOption questionOption = null;
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT QuestionId, Index, IsCorrect, OptionText, OptionAnswerText
                    FROM QuestionOptions
                    WHERE Id = @Id;";

                command.Parameters.AddWithValue("@Id", id);

                var result = await command.ExecuteReaderAsync();
                if (await result.ReadAsync())
                {
                    questionOption = new QuestionOption
                    {
                        Id = id,
                        QuestionId = result.GetInt32(result.GetOrdinal("QuestionId")),
                        Index = result.GetInt32(result.GetOrdinal("Index")),
                        IsCorrect = result.GetBoolean(result.GetOrdinal("IsCorrect")),
                        OptionText = result.GetString(result.GetOrdinal("OptionText")),
                        OptionAnswerText = result.GetString(result.GetOrdinal("OptionAnswerText")),
                    };
                }
            }
        }

        return questionOption;
    }

    public async Task<IEnumerable<IQuestionOption>> GetAllQuestionOptionsForQuestionAsync(int questionId)
    {
        var questionOptions = new List<IQuestionOption>();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT Id, QuestionId, Index, IsCorrect, OptionText, OptionAnswerText
                    FROM QuestionOptions
                    WHERE QuestionId = @Id;";

                command.Parameters.AddWithValue("@Id", questionId);

                var result = await command.ExecuteReaderAsync();
                if (await result.ReadAsync())
                {
                    questionOptions.Add(new QuestionOption
                    {
                        Id = result.GetInt32(result.GetOrdinal("Id")),
                        QuestionId = result.GetInt32(result.GetOrdinal("QuestionId")),
                        Index = result.GetInt32(result.GetOrdinal("Index")),
                        IsCorrect = result.GetBoolean(result.GetOrdinal("IsCorrect")),
                        OptionText = result.GetString(result.GetOrdinal("OptionText")),
                        OptionAnswerText = result.GetString(result.GetOrdinal("OptionAnswerText")),
                    });
                }
            }
        }

        return questionOptions;
    }

    public async Task<IEnumerable<IQuestionOption>> GetAllQuestionOptionsForQuizAsync(int quizzId)
    {
        var questionOptions = new List<IQuestionOption>();
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    SELECT qo.Id, qo.QuestionId, qo.Index, qo.IsCorrect, qo.OptionText, qo.OptionAnswerText
                    FROM QuestionOptions qo
                    INNER JOIN Questions q
                    WHERE q.QuizzId = @Id;";

                command.Parameters.AddWithValue("@Id", quizzId);

                var result = await command.ExecuteReaderAsync();
                if (await result.ReadAsync())
                {
                    questionOptions.Add(new QuestionOption
                    {
                        Id = result.GetInt32(result.GetOrdinal("Id")),
                        QuestionId = result.GetInt32(result.GetOrdinal("QuestionId")),
                        Index = result.GetInt32(result.GetOrdinal("Index")),
                        IsCorrect = result.GetBoolean(result.GetOrdinal("IsCorrect")),
                        OptionText = result.GetString(result.GetOrdinal("OptionText")),
                        OptionAnswerText = result.GetString(result.GetOrdinal("OptionAnswerText")),
                    });
                }
            }
        }

        return questionOptions;
    }

    public async Task UpdateQuestionOptionAsync(IQuestionOption questionOption)
    {
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE QuestionOptions
                    SET QuestionId = @QuestionId, Index = @Index, IsCorrect = @IsCorrect, OptionText = @OptionText, OptionAnswerText = @OptionAnswerText
                    WHERE Id = @Id
                    ";

                command.Parameters.AddWithValue("@QuestionId", questionOption.QuestionId);
                command.Parameters.AddWithValue("@Index", questionOption.Index);
                command.Parameters.AddWithValue("@IsCorrect", questionOption.IsCorrect);
                command.Parameters.AddWithValue("@OptionText", questionOption.OptionText);
                command.Parameters.AddWithValue("@OptionAnswerText", questionOption.OptionAnswerText);

                var result = await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task DeleteQuestionOptionAsync(int id)
    {
         using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    DELETE QuestionOptions
                    WHERE Id = @Id
                    ";

                command.Parameters.AddWithValue("@Id", id);
                var result = await command.ExecuteNonQueryAsync();
            }
        }
    }
}