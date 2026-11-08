using Clinica.Dominio.Eventos;

namespace Clinica.Dominio.Entidades;

/// <summary>
/// O que toda entidade da clínica tem em comum: identidade, quando foi
/// criada, se foi removida logicamente, e a lista de eventos de domínio que
/// ela acumulou.
///
/// Os eventos ficam guardados na entidade e só são publicados depois que o
/// SaveChanges deu certo — quem faz isso é o Unit of Work. Publicar antes
/// significaria avisar o paciente de uma confirmação que a transação ainda
/// pode desfazer.
///
/// Nada aqui se apaga de verdade: numa clínica o histórico é o registro
/// clínico do paciente.
/// </summary>
public abstract class EntidadeBase
{
    private readonly List<IEventoDeDominio> _eventos = new();

    public int Id { get; protected set; }

    public DateTime DataCriacao { get; protected set; } = DateTime.Now;

    public bool EstaDeletado { get; private set; }

    /// <summary>Eventos ainda não publicados.</summary>
    public IReadOnlyCollection<IEventoDeDominio> Eventos => _eventos.AsReadOnly();

    public void Deletar() => EstaDeletado = true;

    public void Restaurar() => EstaDeletado = false;

    protected void RegistrarEvento(IEventoDeDominio evento) => _eventos.Add(evento);

    /// <summary>
    /// Chamado pelo Unit of Work depois de recolher os eventos. Sem isso, um
    /// segundo SaveChanges no mesmo escopo republicaria tudo.
    /// </summary>
    public void LimparEventos() => _eventos.Clear();
}
