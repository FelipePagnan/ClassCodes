using Clinica.Console.Entidades;
using Clinica.Console.Excecoes;
using Clinica.Console.Util;

namespace Clinica.Console.Servicos;

/// <summary>CRUD de médicos.</summary>
public class MedicoService
{
    private readonly List<Medico> _medicos = new();
    private int _proximoId = 1;

    public IReadOnlyList<Medico> ListarTodos()
        => _medicos.Where(m => !m.EstaDeletado).ToList().AsReadOnly();

    public Medico? BuscarPorId(int id)
        => _medicos.FirstOrDefault(m => m.Id == id && !m.EstaDeletado);

    public Medico ObterPorId(int id)
        => BuscarPorId(id) ?? throw new RegraNegocioException($"Médico {id} não encontrado.");

    /// <summary>
    /// Busca pelo CRM. O número só é único dentro da UF, então os dois
    /// precisam ser informados.
    /// </summary>
    public Medico? BuscarPorCrm(string crm, string uf)
    {
        var digitos = Documentos.SomenteDigitos(crm);
        var ufNormalizada = (uf ?? string.Empty).Trim().ToUpperInvariant();

        return _medicos.FirstOrDefault(m =>
            m.Crm == digitos && m.UfCrm == ufNormalizada && !m.EstaDeletado);
    }

    public IEnumerable<Medico> BuscarPorEspecialidade(string especialidade)
        => ListarTodos().Where(m =>
            m.Especialidade.Contains(especialidade.Trim(), StringComparison.OrdinalIgnoreCase));

    public Medico Cadastrar(Medico medico)
    {
        ValidarDados(medico);

        var crm = Documentos.SomenteDigitos(medico.Crm);
        var uf = medico.UfCrm.Trim().ToUpperInvariant();

        // Regra de negócio: o par CRM + UF identifica o médico.
        if (BuscarPorCrm(crm, uf) is not null)
            throw new RegraNegocioException($"Já existe um médico cadastrado com CRM/{uf} {crm}.");

        medico.Id = _proximoId++;
        medico.Crm = crm;
        medico.UfCrm = uf;
        medico.NomeCompleto = medico.NomeCompleto.Trim();
        medico.Especialidade = medico.Especialidade.Trim();
        medico.Celular = string.IsNullOrWhiteSpace(medico.Celular)
            ? null
            : Documentos.SomenteDigitos(medico.Celular);
        medico.Email = string.IsNullOrWhiteSpace(medico.Email)
            ? null
            : medico.Email.Trim().ToLowerInvariant();
        medico.DataCriacao = DateTime.Now;
        medico.EstaDeletado = false;

        _medicos.Add(medico);
        return medico;
    }

    public Medico Atualizar(int id, Medico dados)
    {
        var medico = ObterPorId(id);
        ValidarDados(dados);

        var crm = Documentos.SomenteDigitos(dados.Crm);
        var uf = dados.UfCrm.Trim().ToUpperInvariant();

        var existente = BuscarPorCrm(crm, uf);
        if (existente is not null && existente.Id != id)
            throw new RegraNegocioException($"Já existe outro médico com CRM/{uf} {crm}.");

        medico.NomeCompleto = dados.NomeCompleto.Trim();
        medico.Crm = crm;
        medico.UfCrm = uf;
        medico.Especialidade = dados.Especialidade.Trim();
        medico.Celular = string.IsNullOrWhiteSpace(dados.Celular)
            ? null
            : Documentos.SomenteDigitos(dados.Celular);
        medico.Email = string.IsNullOrWhiteSpace(dados.Email)
            ? null
            : dados.Email.Trim().ToLowerInvariant();

        return medico;
    }

    public void Remover(int id)
    {
        var medico = ObterPorId(id);

        var abertos = medico.AtendimentosAtivos.Count(a => a.EstaAberto);
        if (abertos > 0)
            throw new RegraNegocioException(
                $"Dr(a). {medico.NomeCompleto} tem {abertos} atendimento(s) em aberto. " +
                "Cancele ou remarque antes de remover o médico.");

        medico.EstaDeletado = true;
    }

    private static void ValidarDados(Medico medico)
    {
        if (string.IsNullOrWhiteSpace(medico.NomeCompleto))
            throw new RegraNegocioException("O nome completo é obrigatório.");

        if (medico.NomeCompleto.Trim().Length < 3)
            throw new RegraNegocioException("O nome completo deve ter ao menos 3 caracteres.");

        if (!Documentos.CrmValido(medico.Crm))
            throw new RegraNegocioException(
                $"O CRM '{medico.Crm}' é inválido. Informe de 4 a 7 dígitos.");

        if (!Documentos.UfValida(medico.UfCrm))
            throw new RegraNegocioException(
                $"A UF '{medico.UfCrm}' é inválida. Use a sigla do estado, ex: SP.");

        if (string.IsNullOrWhiteSpace(medico.Especialidade))
            throw new RegraNegocioException("A especialidade é obrigatória.");

        if (!string.IsNullOrWhiteSpace(medico.Celular) && !Documentos.CelularValido(medico.Celular))
            throw new RegraNegocioException($"O celular '{medico.Celular}' é inválido.");

        if (!string.IsNullOrWhiteSpace(medico.Email) && !medico.Email.Contains('@'))
            throw new RegraNegocioException($"O e-mail '{medico.Email}' é inválido.");
    }
}
