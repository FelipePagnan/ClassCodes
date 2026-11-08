namespace Clinica.Dominio.Enums;

/// <summary>Situação de um atendimento ao longo do ciclo de vida.</summary>
public enum StatusAtendimento
{
    Agendado = 1,
    Confirmado = 2,
    Realizado = 3,
    Cancelado = 4,
    NaoCompareceu = 5
}

/// <summary>Tipo de documento anexado a um atendimento.</summary>
public enum TipoAnexo
{
    Atestado = 1,
    Receita = 2,
    Evolucao = 3
}

/// <summary>Perfis de acesso ao sistema.</summary>
public enum PerfilUsuario
{
    Administrador = 1,
    Medico = 2,
    Recepcionista = 3
}

public enum Sexo
{
    Masculino = 1,
    Feminino = 2,
    Outro = 3
}
