namespace Clinica.Api.Dominio.Entidades;

/// <summary>
/// O que toda entidade da clínica tem em comum: identidade, quando foi
/// criada e se foi removida logicamente.
///
/// Nada aqui se apaga de verdade. Numa clínica o histórico é o registro
/// clínico do paciente — removeu-se da tela, não do banco.
/// </summary>
public abstract class EntidadeBase
{
    public int Id { get; protected set; }

    public DateTime DataCriacao { get; protected set; } = DateTime.Now;

    public bool EstaDeletado { get; private set; }

    public void Deletar() => EstaDeletado = true;

    public void Restaurar() => EstaDeletado = false;
}
