CREATE TABLE [dbo].[Imovel]
(
	[Id] VARCHAR(300) NOT NULL, 
    [Origem] VARCHAR(100) NOT NULL, 
    [DataInclusao] DATETIME2 NOT NULL, 
    [DataModificacao] DATETIME2 NULL, 
    [Link] VARCHAR(300) NULL, 
    [Titulo] VARCHAR(300) NULL, 
    [Endereco] VARCHAR(300) NULL, 
    [ValorAluguel] VARCHAR(100) NULL, 
    [ValorCondominio] VARCHAR(100) NULL, 
    [Quartos] VARCHAR(100) NULL, 
    [Vagas] VARCHAR(100) NULL, 
    [Iptu] VARCHAR(100) NULL,

    PRIMARY KEY (Id, Origem)
)