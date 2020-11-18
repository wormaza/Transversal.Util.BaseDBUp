CREATE TABLE A
(id int identity not null,a int null)
GO;

ALTER TABLE A
ADD CONSTRAINT PK_A
PRIMARY KEY (id)
GO;