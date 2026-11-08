using Clinica.Api.Data;
using Clinica.Api.Entidades;
using Clinica.Api.Excecoes;
using Clinica.Api.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Api.Servicos;

/// <summary>CRUD dos serviços oferecidos pela clínica.</summary>
public class ServicoService
{
    private readonly ClinicaDbContext _context;

    public ServicoService(ClinicaDbContext context) => _context = context;

    public async Task<List<Servico>> ListarAsync()
        => await _context.Servicos
            .Where(s => !s.EstaDeletado)
            .OrderBy(s => s.Nome)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Servico?> BuscarPorIdAsync(int id)
        => await _context.Servicos.FirstOrDefaultAsync(s => s.Id == id && !s.EstaDeletado);

    public async Task<Servico> ObterPorIdAsync(int id)
        => await BuscarPorIdAsync(id)
           ?? throw new RegraNegocioException($"Serviço {id} não encontrado.");

    public async Task<Servico> CadastrarAsync(ServicoRequisicao requisicao)
    {
        Validar(requisicao);

        var nome = requisicao.Nome.Trim();

        if (await _context.Servicos.AnyAsync(s => s.Nome == nome && !s.EstaDeletado))
            throw new RegraNegocioException($"Já existe um serviço chamado '{nome}'.");

        var servico = new Servico
        {
            Nome = nome,
            Descricao = string.IsNullOrWhiteSpace(requisicao.Descricao)
                ? null
                : requisicao.Descricao.Trim(),
            Valor = requisicao.Valor,
            DuracaoMinutos = requisicao.DuracaoMinutos,
            DataCriacao = DateTime.Now
        };

        _context.Servicos.Add(servico);
        await _context.SaveChangesAsync();

        return servico;
    }

    public async Task<Servico> AtualizarAsync(int id, ServicoRequisicao requisicao)
    {
        var servico = await ObterPorIdAsync(id);
        Validar(requisicao);

        var nome = requisicao.Nome.Trim();

        if (await _context.Servicos.AnyAsync(s =>
                s.Nome == nome && s.Id != id && !s.EstaDeletado))
            throw new RegraNegocioException($"Já existe outro serviço chamado '{nome}'.");

        // Mudar valor e duração aqui não mexe nos atendimentos já marcados:
        // eles guardam a própria cópia desses dois campos.
        servico.Nome = nome;
        servico.Descricao = string.IsNullOrWhiteSpace(requisicao.Descricao)
            ? null
            : requisicao.Descricao.Trim();
        servico.Valor = requisicao.Valor;
        servico.DuracaoMinutos = requisicao.DuracaoMinutos;

        await _context.SaveChangesAsync();
        return servico;
    }

    public async Task RemoverAsync(int id)
    {
        var servico = await _context.Servicos
                          .Include(s => s.Atendimentos)
                          .FirstOrDefaultAsync(s => s.Id == id && !s.EstaDeletado)
                      ?? throw new RegraNegocioException($"Serviço {id} não encontrado.");

        var abertos = servico.Atendimentos.Count(a => !a.EstaDeletado && a.EstaAberto);

        if (abertos > 0)
            throw new RegraNegocioException(
                $"O serviço '{servico.Nome}' tem {abertos} atendimento(s) em aberto.");

        servico.EstaDeletado = true;
        await _context.SaveChangesAsync();
    }

    private static void Validar(ServicoRequisicao requisicao)
    {
        if (string.IsNullOrWhiteSpace(requisicao.Nome) || requisicao.Nome.Trim().Length < 3)
            throw new RegraNegocioException("O nome do serviço deve ter ao menos 3 caracteres.");

        if (requisicao.Nome.Trim().Length > 150)
            throw new RegraNegocioException("O nome do serviço deve ter no máximo 150 caracteres.");

        if (requisicao.Valor < 0)
            throw new RegraNegocioException("O valor do serviço não pode ser negativo.");

        // decimal.Round compara por valor, então 150.00 passa e 150.005 não.
        if (decimal.Round(requisicao.Valor, 2) != requisicao.Valor)
            throw new RegraNegocioException("O valor deve ter no máximo 2 casas decimais.");

        if (requisicao.DuracaoMinutos <= 0)
            throw new RegraNegocioException("A duração deve ser maior que zero.");

        if (requisicao.DuracaoMinutos > 480)
            throw new RegraNegocioException("A duração não pode passar de 8 horas (480 minutos).");
    }
}
