CREATE DATABASE CoinGeckoDb;
GO

USE CoinGeckoDb;
GO

CREATE TABLE Cotacoes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DataHora DATETIME NOT NULL DEFAULT GETDATE(),
    Preco DECIMAL(18,2) NOT NULL,
    Variacao DECIMAL(8,4) NULL
);
GO

select * from Cotacoes


CREATE VIEW vw_ResumoCotas AS
SELECT FORMAT(DataHora, 'dd/MM/yyyy HH:mm') [Data da Cotação], Preco [Preço Atual], Variacao [Variação do Bitcoin],
dbo.retorna_status_variacao(Variacao) [Status da Variação], MAX(Preco) OVER() [Maior Preço já Registrado]
FROM Cotacoes

SELECT * FROM vw_ResumoCotas

CREATE OR ALTER FUNCTION retorna_status_variacao(@variacao DECIMAL)
RETURNS VARCHAR (10)
AS 
BEGIN
    DECLARE @resultado VARCHAR(10)
    SET @resultado = CASE
    WHEN @variacao > 0 THEN 'ALTA'
    WHEN @variacao < 0 THEN 'BAIXA'
    ELSE 'ESTÁVEL'
    END;
    RETURN @resultado;
END;