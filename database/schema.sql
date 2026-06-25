CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    email TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    first_name TEXT NOT NULL,
    surname TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE quiz (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    type INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by INT REFERENCES users(id)
);

CREATE TABLE questions (
    id SERIAL PRIMARY KEY,
    quiz_id INT REFERENCES quiz(id),
    index INT NOT NULL,
    type INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_by INT REFERENCES users(id),
    question_text TEXT NOT NULL
);

CREATE TABLE question_options (
    id SERIAL PRIMARY KEY,
    question_id INT REFERENCES questions(id),
    index INT NOT NULL,
    is_correct BOOLEAN NOT NULL DEFAULT FALSE,
    option_text TEXT NOT NULL,
    option_answer_text TEXT NOT NULL
);

