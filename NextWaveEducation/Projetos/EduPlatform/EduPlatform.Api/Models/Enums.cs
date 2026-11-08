namespace EduPlatform.Api.Models;

public enum RoleUsuario
{
    Aluno = 0,
    Instrutor = 1,
    Admin = 2
}

public enum StatusUsuarioAssinatura
{
    Pendente = 0,
    Ativo = 1,
    Desativado = 2,
    Expirado = 3
}

public enum StatusPagamento
{
    Sucesso = 0,
    Falha = 1,
    Pendente = 2,
    Atrasado = 3
}
