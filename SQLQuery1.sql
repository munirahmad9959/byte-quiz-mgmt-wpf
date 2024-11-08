CREATE TABLE users (
    Id INT IDENTITY(1,1) PRIMARY KEY,  
    username VARCHAR(50) UNIQUE,        
    password VARCHAR(100)
);


INSERT INTO users (username, password)
VALUES 
    ('munir', '123'),
    ('sohaib', '124'),
    ('ali', '125'),
    ('abrar', '126'),
    ('abbas', '127');

