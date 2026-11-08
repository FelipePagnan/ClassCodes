using System.Globalization;
using Caixinhas.Console.Entidades;
using Caixinhas.Console.Enums;
using Caixinhas.Console.Excecoes;
using Caixinhas.Console.Servicos;

namespace Caixinhas.Console;

public static class Program
{
    private static readonly ObjetivoService _objetivoService = new();
    private static readonly TransacaoService _transacaoService = new(_objetivoService);
    private static readonly CultureInfo _cultura = CultureInfo.InvariantCulture;

    public static void Main(string[] args)
    {
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;
        CarregarDadosDeExemplo();

        var sair = false;
        while (!sair)
        {
            ExibirMenu();
            var opcao = System.Console.ReadLine();
            System.Console.WriteLine();

            // Entrada encerrada (Ctrl+D ou stdin redirecionado): encerra o programa.
            if (opcao is null) break;

            try
            {
                switch (opcao.Trim())
                {
                    case "1": CadastrarObjetivo(); break;
                    case "2": ListarObjetivos(); break;
                    case "3": DetalharObjetivo(); break;
                    case "4": AtualizarObjetivo(); break;
                    case "5": RemoverObjetivo(); break;
                    case "6": DefinirCapa(); break;
                    case "7": AlterarStatus(); break;
                    case "8": CadastrarTransacao(); break;
                    case "9": ListarTransacoes(); break;
                    case "10": ExibirExtrato(); break;
                    case "11": RemoverTransacao(); break;
                    case "12": SimularEvolucao(); break;
                    case "13": RelatorioEvolucao(); break;
                    case "0": sair = true; break;
                    default:
                        System.Console.WriteLine("Opção inválida.");
                        break;
                }
            }
            catch (RegraNegocioException ex)
            {
                System.Console.WriteLine($"[Regra de negócio] {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[Erro inesperado] {ex.Message}");
            }

            if (!sair)
            {
                System.Console.WriteLine("\nPressione ENTER para continuar...");
                if (System.Console.ReadLine() is null) break;
            }
        }

        System.Console.WriteLine("Encerrando. Bons investimentos!");
    }

    private static void ExibirMenu()
    {
        System.Console.Clear();
        System.Console.WriteLine("============================================================");
        System.Console.WriteLine(" GERENCIADOR DE OBJETIVOS FINANCEIROS - Nível Básico I");
        System.Console.WriteLine("============================================================");
        System.Console.WriteLine(" CAIXINHAS");
        System.Console.WriteLine("  1 - Cadastrar objetivo");
        System.Console.WriteLine("  2 - Listar objetivos");
        System.Console.WriteLine("  3 - Detalhes de um objetivo");
        System.Console.WriteLine("  4 - Atualizar objetivo");
        System.Console.WriteLine("  5 - Remover objetivo");
        System.Console.WriteLine("  6 - Definir imagem de capa");
        System.Console.WriteLine("  7 - Pausar / retomar / cancelar objetivo");
        System.Console.WriteLine(" TRANSAÇÕES");
        System.Console.WriteLine("  8 - Cadastrar transação (depósito / saque)");
        System.Console.WriteLine("  9 - Listar transações de um objetivo");
        System.Console.WriteLine(" 10 - Extrato com saldo corrido");
        System.Console.WriteLine(" 11 - Remover transação");
        System.Console.WriteLine(" SIMULAÇÃO E RELATÓRIOS");
        System.Console.WriteLine(" 12 - Simular evolução de um objetivo");
        System.Console.WriteLine(" 13 - Relatório de evolução das caixinhas");
        System.Console.WriteLine("  0 - Sair");
        System.Console.Write("\nEscolha uma opção: ");
    }

    // ----------------------------- CAIXINHAS -----------------------------

    private static void CadastrarObjetivo()
    {
        System.Console.WriteLine("--- Cadastro de Objetivo Financeiro ---");

        var objetivo = new ObjetivoFinanceiro
        {
            Titulo = Perguntar("Título (ex: Viagem para o Japão)"),
            QuantidadeAlvo = PerguntarDecimal("Quantidade alvo (R$)")
        };

        System.Console.Write("Prazo (dd/MM/yyyy, ENTER para nenhum): ");
        var prazoTexto = System.Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(prazoTexto))
        {
            if (!DateTime.TryParseExact(prazoTexto, "dd/MM/yyyy", _cultura,
                    DateTimeStyles.None, out var prazo))
                throw new RegraNegocioException($"'{prazoTexto}' não é uma data válida (dd/MM/yyyy).");

            objetivo.Prazo = prazo;
        }

        System.Console.Write("Aporte mensal ideal (R$, ENTER para nenhum): ");
        var aporteTexto = System.Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(aporteTexto))
        {
            if (!decimal.TryParse(aporteTexto, NumberStyles.Number, _cultura, out var aporte))
                throw new RegraNegocioException($"'{aporteTexto}' não é um valor válido.");

            objetivo.QuantidadeIdealAporteMensal = aporte;
        }

        var cadastrado = _objetivoService.Cadastrar(objetivo);
        System.Console.WriteLine($"\nObjetivo cadastrado!\n  {cadastrado}");
    }

    private static void ListarObjetivos()
    {
        var objetivos = _objetivoService.ListarTodos();

        if (objetivos.Count == 0)
        {
            System.Console.WriteLine("Nenhum objetivo cadastrado.");
            return;
        }

        System.Console.WriteLine("--- Objetivos Financeiros ---");
        foreach (var objetivo in objetivos.OrderByDescending(o => o.PercentualConcluido))
            System.Console.WriteLine($"  {objetivo}");

        System.Console.WriteLine(
            $"\n  Patrimônio total: R$ {objetivos.Sum(o => o.ValorTotal):N2}" +
            $"  |  Soma das metas: R$ {objetivos.Sum(o => o.QuantidadeAlvo):N2}");
    }

    private static void DetalharObjetivo()
    {
        ListarObjetivos();
        var id = PerguntarInteiro("\nId do objetivo");
        var objetivo = _objetivoService.ObterPorId(id);

        System.Console.WriteLine($"\n--- {objetivo.Titulo} ---");
        System.Console.WriteLine($"  Status................: {objetivo.Status}");
        System.Console.WriteLine($"  Saldo atual...........: R$ {objetivo.ValorTotal:N2}");
        System.Console.WriteLine($"  Meta..................: R$ {objetivo.QuantidadeAlvo:N2}");
        System.Console.WriteLine($"  Falta.................: R$ {objetivo.ValorRestante:N2}");
        System.Console.WriteLine($"  Progresso.............: {objetivo.PercentualConcluido:N2}%  {BarraDeProgresso(objetivo.PercentualConcluido)}");
        System.Console.WriteLine($"  Prazo.................: {(objetivo.Prazo is null ? "sem prazo" : objetivo.Prazo.Value.ToString("dd/MM/yyyy"))}");

        if (objetivo.DiasRestantes is { } dias)
            System.Console.WriteLine($"  Dias restantes........: {dias}{(objetivo.PrazoVencido ? "  [PRAZO VENCIDO]" : string.Empty)}");

        if (objetivo.QuantidadeIdealAporteMensal is { } ideal)
            System.Console.WriteLine($"  Aporte mensal ideal...: R$ {ideal:N2}");

        if (objetivo.AporteMensalNecessario is { } necessario)
            System.Console.WriteLine($"  Aporte p/ bater a meta: R$ {necessario:N2}/mês");

        var transacoes = objetivo.TransacoesAtivas.ToList();
        System.Console.WriteLine($"  Transações............: {transacoes.Count}" +
            $" ({transacoes.Count(t => t.Tipo == TipoTransacao.Deposito)} depósitos, " +
            $"{transacoes.Count(t => t.Tipo == TipoTransacao.Saque)} saques)");
        System.Console.WriteLine($"  Capa..................: {(objetivo.TemCapa ? $"{objetivo.CapaObjetivo!.Length} bytes" : "sem capa")}");
        System.Console.WriteLine($"  Criado em.............: {objetivo.DataCriacao:dd/MM/yyyy HH:mm}");

        if (transacoes.Count == 0)
        {
            System.Console.WriteLine("\n  Nenhuma movimentação ainda.");
            return;
        }

        System.Console.WriteLine("\n  Últimas movimentações:");
        foreach (var transacao in transacoes.OrderByDescending(t => t.DataTransacao).Take(5))
            System.Console.WriteLine($"    {transacao}");
    }

    private static void AtualizarObjetivo()
    {
        ListarObjetivos();
        var id = PerguntarInteiro("\nId do objetivo a atualizar");
        var atual = _objetivoService.ObterPorId(id);

        System.Console.WriteLine($"\nAtualizando '{atual.Titulo}'. ENTER mantém o valor atual.");

        var titulo = PerguntarComPadrao("Título", atual.Titulo);
        var alvo = PerguntarDecimalComPadrao("Quantidade alvo (R$)", atual.QuantidadeAlvo);

        System.Console.Write($"Prazo (dd/MM/yyyy) [{(atual.Prazo is null ? "sem prazo" : atual.Prazo.Value.ToString("dd/MM/yyyy"))}]: ");
        var prazoTexto = System.Console.ReadLine();
        DateTime? prazo = atual.Prazo;
        if (!string.IsNullOrWhiteSpace(prazoTexto))
        {
            if (!DateTime.TryParseExact(prazoTexto, "dd/MM/yyyy", _cultura,
                    DateTimeStyles.None, out var novoPrazo))
                throw new RegraNegocioException($"'{prazoTexto}' não é uma data válida (dd/MM/yyyy).");

            prazo = novoPrazo;
        }

        var aporte = atual.QuantidadeIdealAporteMensal;
        System.Console.Write($"Aporte mensal ideal (R$) [{(aporte is null ? "nenhum" : aporte.Value.ToString("N2"))}]: ");
        var aporteTexto = System.Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(aporteTexto))
        {
            if (!decimal.TryParse(aporteTexto, NumberStyles.Number, _cultura, out var novoAporte))
                throw new RegraNegocioException($"'{aporteTexto}' não é um valor válido.");

            aporte = novoAporte;
        }

        var atualizado = _objetivoService.Atualizar(id, titulo, alvo, prazo, aporte);
        System.Console.WriteLine($"\nObjetivo atualizado!\n  {atualizado}");
    }

    private static void RemoverObjetivo()
    {
        ListarObjetivos();
        var id = PerguntarInteiro("\nId do objetivo a remover");
        var objetivo = _objetivoService.ObterPorId(id);

        System.Console.Write($"Confirma a remoção de '{objetivo.Titulo}'? (s/n): ");
        if (!(System.Console.ReadLine() ?? "n").Trim().Equals("s", StringComparison.OrdinalIgnoreCase))
        {
            System.Console.WriteLine("Remoção cancelada.");
            return;
        }

        _objetivoService.Remover(id);
        System.Console.WriteLine("Objetivo removido (remoção lógica, o histórico foi preservado).");
    }

    private static void DefinirCapa()
    {
        ListarObjetivos();
        var id = PerguntarInteiro("\nId do objetivo");
        var caminho = Perguntar("Caminho do arquivo de imagem");

        var objetivo = _objetivoService.DefinirCapa(id, caminho);
        System.Console.WriteLine($"\nCapa definida: {objetivo.CapaObjetivo!.Length} bytes armazenados.");
    }

    private static void AlterarStatus()
    {
        ListarObjetivos();
        var id = PerguntarInteiro("\nId do objetivo");
        var objetivo = _objetivoService.ObterPorId(id);

        System.Console.WriteLine($"\nStatus atual de '{objetivo.Titulo}': {objetivo.Status}");
        System.Console.WriteLine("1 - Pausar");
        System.Console.WriteLine("2 - Retomar");
        System.Console.WriteLine("3 - Cancelar");

        switch (Perguntar("Opção").Trim())
        {
            case "1":
                objetivo.Pausar();
                System.Console.WriteLine($"Objetivo pausado. Status: {objetivo.Status}");
                break;
            case "2":
                objetivo.Retomar();
                System.Console.WriteLine($"Objetivo retomado. Status: {objetivo.Status}");
                break;
            case "3":
                objetivo.Cancelar();
                System.Console.WriteLine($"Objetivo cancelado. Status: {objetivo.Status}");
                break;
            default:
                System.Console.WriteLine("Opção inválida.");
                break;
        }
    }

    // ----------------------------- TRANSAÇÕES -----------------------------

    private static void CadastrarTransacao()
    {
        System.Console.WriteLine("--- Cadastro de Transação ---");
        ListarObjetivos();

        var idObjetivo = PerguntarInteiro("\nId do objetivo");
        var objetivo = _objetivoService.ObterPorId(idObjetivo);
        System.Console.WriteLine($"Saldo atual de '{objetivo.Titulo}': R$ {objetivo.ValorTotal:N2}");

        System.Console.WriteLine("\n1 - Depósito");
        System.Console.WriteLine("2 - Saque");
        var tipoTexto = Perguntar("Tipo").Trim();

        var tipo = tipoTexto switch
        {
            "1" => TipoTransacao.Deposito,
            "2" => TipoTransacao.Saque,
            _ => throw new RegraNegocioException("Tipo de transação inválido.")
        };

        var quantidade = PerguntarDecimal("Quantidade (R$, até 2 casas decimais)");

        System.Console.Write("Data da transação (dd/MM/yyyy, ENTER para hoje): ");
        var dataTexto = System.Console.ReadLine();
        var data = DateTime.Today;
        if (!string.IsNullOrWhiteSpace(dataTexto))
        {
            if (!DateTime.TryParseExact(dataTexto, "dd/MM/yyyy", _cultura,
                    DateTimeStyles.None, out data))
                throw new RegraNegocioException($"'{dataTexto}' não é uma data válida (dd/MM/yyyy).");
        }

        var transacao = _transacaoService.Cadastrar(idObjetivo, quantidade, tipo, data);

        System.Console.WriteLine($"\nTransação registrada!\n  {transacao}");
        System.Console.WriteLine($"  Novo saldo de '{objetivo.Titulo}': R$ {objetivo.ValorTotal:N2} " +
            $"({objetivo.PercentualConcluido:N2}% da meta)");

        if (objetivo.Status == StatusObjetivo.Completo)
            System.Console.WriteLine($"\n  *** META ALCANÇADA! '{objetivo.Titulo}' está completo. ***");
    }

    private static void ListarTransacoes()
    {
        ListarObjetivos();
        var id = PerguntarInteiro("\nId do objetivo");
        var objetivo = _objetivoService.ObterPorId(id);
        var transacoes = _transacaoService.ListarPorObjetivo(id).ToList();

        System.Console.WriteLine($"\n--- Transações de '{objetivo.Titulo}' ---");

        if (transacoes.Count == 0)
        {
            System.Console.WriteLine("  Nenhuma transação cadastrada.");
            return;
        }

        foreach (var transacao in transacoes)
            System.Console.WriteLine($"  {transacao}");

        System.Console.WriteLine($"\n  Saldo: R$ {objetivo.ValorTotal:N2}");
    }

    private static void ExibirExtrato()
    {
        ListarObjetivos();
        var id = PerguntarInteiro("\nId do objetivo");
        var objetivo = _objetivoService.ObterPorId(id);
        var extrato = _transacaoService.GerarExtrato(id);

        System.Console.WriteLine($"\n--- Extrato de '{objetivo.Titulo}' ---");

        if (extrato.Count == 0)
        {
            System.Console.WriteLine("  Nenhuma movimentação.");
            return;
        }

        System.Console.WriteLine($"  {"Data",-12}{"Tipo",-10}{"Valor",15}{"Saldo",15}");
        System.Console.WriteLine($"  {new string('-', 52)}");

        foreach (var linha in extrato)
        {
            System.Console.WriteLine(
                $"  {linha.Data,-12:dd/MM/yyyy}{linha.Tipo,-10}" +
                $"{linha.Quantidade,15:N2}{linha.SaldoApos,15:N2}");
        }

        System.Console.WriteLine($"  {new string('-', 52)}");
        System.Console.WriteLine($"  {"SALDO FINAL",-22}{objetivo.ValorTotal,30:N2}");
    }

    private static void RemoverTransacao()
    {
        ListarObjetivos();
        var idObjetivo = PerguntarInteiro("\nId do objetivo");
        var transacoes = _transacaoService.ListarPorObjetivo(idObjetivo).ToList();

        if (transacoes.Count == 0)
        {
            System.Console.WriteLine("Este objetivo não tem transações.");
            return;
        }

        System.Console.WriteLine("\nTransações:");
        foreach (var transacao in transacoes)
            System.Console.WriteLine($"  {transacao}");

        var idTransacao = PerguntarInteiro("\nId da transação a remover");
        var objetivo = _objetivoService.ObterPorId(idObjetivo);

        _transacaoService.Remover(idTransacao);

        System.Console.WriteLine("Transação removida (remoção lógica).");
        System.Console.WriteLine($"  Novo saldo de '{objetivo.Titulo}': R$ {objetivo.ValorTotal:N2}");
    }

    // ----------------------- SIMULAÇÃO E RELATÓRIOS -----------------------

    private static void SimularEvolucao()
    {
        System.Console.WriteLine("--- Simulação de Evolução ---");
        ListarObjetivos();

        var id = PerguntarInteiro("\nId do objetivo");
        var objetivo = _objetivoService.ObterPorId(id);

        var aporteSugerido = objetivo.QuantidadeIdealAporteMensal
            ?? objetivo.AporteMensalNecessario
            ?? 500m;

        var aporte = PerguntarDecimalComPadrao("Aporte mensal (R$)", aporteSugerido);
        var meses = PerguntarInteiroComPadrao("Quantidade de meses", 12);
        var taxa = PerguntarDecimalComPadrao("Taxa de juros mensal (%)", 0.80m);

        var simulacao = objetivo.Simular(aporte, meses, taxa);

        System.Console.WriteLine($"\n--- Projeção para '{simulacao.Titulo}' ---");
        System.Console.WriteLine($"  Saldo inicial.....: R$ {simulacao.SaldoInicial:N2}");
        System.Console.WriteLine($"  Meta..............: R$ {simulacao.QuantidadeAlvo:N2}");
        System.Console.WriteLine($"  Aporte mensal.....: R$ {simulacao.AporteMensal:N2}");
        System.Console.WriteLine($"  Taxa mensal.......: {simulacao.TaxaJurosMensalPercentual:N2}%");
        System.Console.WriteLine();
        System.Console.WriteLine($"  {"Mês",-6}{"Data",-12}{"Aporte",13}{"Rendimento",13}{"Saldo",15}{"% Meta",10}");
        System.Console.WriteLine($"  {new string('-', 69)}");

        foreach (var mes in simulacao.Meses)
        {
            System.Console.WriteLine(
                $"  {mes.Mes,-6}{mes.Data,-12:MM/yyyy}{mes.Aporte,13:N2}" +
                $"{mes.Rendimento,13:N2}{mes.SaldoAcumulado,15:N2}{mes.PercentualDaMeta,9:N2}%");
        }

        System.Console.WriteLine($"  {new string('-', 69)}");
        System.Console.WriteLine($"\n  Saldo final.......: R$ {simulacao.SaldoFinal:N2}");
        System.Console.WriteLine($"  Total aportado....: R$ {simulacao.TotalAportado:N2}");
        System.Console.WriteLine($"  Total rendimentos.: R$ {simulacao.TotalRendimentos:N2}");

        if (simulacao.MesQueAtingeMeta is { } mesMeta)
        {
            System.Console.WriteLine(
                $"\n  Meta alcançada no mês {mesMeta} " +
                $"({DateTime.Today.AddMonths(mesMeta):MM/yyyy}).");
        }
        else
        {
            var faltam = simulacao.QuantidadeAlvo - simulacao.SaldoFinal;
            System.Console.WriteLine(
                $"\n  A meta não é alcançada em {simulacao.QuantidadeMeses} meses. " +
                $"Faltariam R$ {faltam:N2}.");
        }
    }

    private static void RelatorioEvolucao()
    {
        var relatorio = _objetivoService.GerarRelatorioEvolucao();

        System.Console.WriteLine("--- Relatório de Evolução das Caixinhas ---");
        System.Console.WriteLine($"  Gerado em.............: {relatorio.GeradoEm:dd/MM/yyyy HH:mm}");
        System.Console.WriteLine($"  Objetivos ativos......: {relatorio.TotalObjetivos}");
        System.Console.WriteLine($"  Completos.............: {relatorio.ObjetivosCompletos}");
        System.Console.WriteLine($"  Em progresso..........: {relatorio.ObjetivosEmProgresso}");
        System.Console.WriteLine($"  Patrimônio total......: R$ {relatorio.PatrimonioTotal:N2}");
        System.Console.WriteLine($"  Soma das metas........: R$ {relatorio.SomaDasMetas:N2}");

        if (relatorio.Objetivos.Count == 0)
        {
            System.Console.WriteLine("\n  Nenhum objetivo para relatar.");
            return;
        }

        System.Console.WriteLine("\n  Por objetivo:");
        foreach (var item in relatorio.Objetivos)
        {
            System.Console.WriteLine(
                $"    {item.Titulo,-28} R$ {item.ValorTotal,12:N2} / R$ {item.QuantidadeAlvo,12:N2} " +
                $"({item.PercentualConcluido,6:N2}%) {item.Status}");
            System.Console.WriteLine(
                $"      {item.QuantidadeDepositos} depósito(s) R$ {item.TotalDepositado:N2}  |  " +
                $"{item.QuantidadeSaques} saque(s) R$ {item.TotalSacado:N2}" +
                $"{(item.PrazoVencido ? "  [PRAZO VENCIDO]" : string.Empty)}");

            if (item.AporteMensalNecessario is { } necessario)
                System.Console.WriteLine($"      Precisa guardar R$ {necessario:N2}/mês para bater a meta.");
        }

        System.Console.WriteLine("\n  Evolução mensal consolidada:");
        System.Console.WriteLine($"    {"Mês",-10}{"Depósitos",14}{"Saques",14}{"Líquido",14}{"Acumulado",15}");
        System.Console.WriteLine($"    {new string('-', 67)}");

        foreach (var mes in relatorio.EvolucaoMensal)
        {
            System.Console.WriteLine(
                $"    {mes.Mes,-10}{mes.Depositos,14:N2}{mes.Saques,14:N2}" +
                $"{mes.Liquido,14:N2}{mes.SaldoAcumulado,15:N2}");
        }
    }

    // ------------------------- ENTRADA DE DADOS ---------------------------

    private static string BarraDeProgresso(decimal percentual, int largura = 20)
    {
        var preenchido = (int)Math.Round(percentual / 100 * largura);
        preenchido = Math.Clamp(preenchido, 0, largura);
        return $"[{new string('#', preenchido)}{new string('.', largura - preenchido)}]";
    }

    private static string Perguntar(string rotulo)
    {
        System.Console.Write($"{rotulo}: ");
        return System.Console.ReadLine() ?? string.Empty;
    }

    private static string PerguntarComPadrao(string rotulo, string padrao)
    {
        System.Console.Write($"{rotulo} [{padrao}]: ");
        var valor = System.Console.ReadLine();
        return string.IsNullOrWhiteSpace(valor) ? padrao : valor;
    }

    private static int PerguntarInteiro(string rotulo)
    {
        System.Console.Write($"{rotulo}: ");
        var entrada = System.Console.ReadLine();

        if (!int.TryParse(entrada, out var valor))
            throw new RegraNegocioException($"'{entrada}' não é um número inteiro válido.");

        return valor;
    }

    private static int PerguntarInteiroComPadrao(string rotulo, int padrao)
    {
        System.Console.Write($"{rotulo} [{padrao}]: ");
        var entrada = System.Console.ReadLine();

        if (string.IsNullOrWhiteSpace(entrada)) return padrao;

        if (!int.TryParse(entrada, out var valor))
            throw new RegraNegocioException($"'{entrada}' não é um número inteiro válido.");

        return valor;
    }

    private static decimal PerguntarDecimal(string rotulo)
    {
        System.Console.Write($"{rotulo}: ");
        var entrada = System.Console.ReadLine();

        if (!decimal.TryParse(entrada, NumberStyles.Number, _cultura, out var valor))
            throw new RegraNegocioException(
                $"'{entrada}' não é um valor válido. Use ponto como separador decimal (ex: 1500.50).");

        return valor;
    }

    private static decimal PerguntarDecimalComPadrao(string rotulo, decimal padrao)
    {
        System.Console.Write($"{rotulo} [{padrao:N2}]: ");
        var entrada = System.Console.ReadLine();

        if (string.IsNullOrWhiteSpace(entrada)) return padrao;

        if (!decimal.TryParse(entrada, NumberStyles.Number, _cultura, out var valor))
            throw new RegraNegocioException(
                $"'{entrada}' não é um valor válido. Use ponto como separador decimal (ex: 1500.50).");

        return valor;
    }

    /// <summary>
    /// Carrega dados de exemplo para facilitar os testes manuais.
    /// </summary>
    private static void CarregarDadosDeExemplo()
    {
        var hoje = DateTime.Today;

        var viagem = _objetivoService.Cadastrar(new ObjetivoFinanceiro
        {
            Titulo = "Viagem para o Japão",
            QuantidadeAlvo = 20_000m,
            Prazo = hoje.AddMonths(18),
            QuantidadeIdealAporteMensal = 1_200m
        });

        var reserva = _objetivoService.Cadastrar(new ObjetivoFinanceiro
        {
            Titulo = "Reserva de emergência",
            QuantidadeAlvo = 30_000m,
            QuantidadeIdealAporteMensal = 2_000m
        });

        var notebook = _objetivoService.Cadastrar(new ObjetivoFinanceiro
        {
            Titulo = "Notebook novo",
            QuantidadeAlvo = 8_000m,
            Prazo = hoje.AddMonths(4)
        });

        // As caixinhas foram criadas hoje, então as transações de exemplo
        // usam a data de hoje (a regra impede data anterior à criação).
        _transacaoService.Cadastrar(viagem.Id, 1_200m, TipoTransacao.Deposito, hoje);
        _transacaoService.Cadastrar(viagem.Id, 1_500.50m, TipoTransacao.Deposito, hoje);
        _transacaoService.Cadastrar(viagem.Id, 300m, TipoTransacao.Saque, hoje);

        _transacaoService.Cadastrar(reserva.Id, 2_000m, TipoTransacao.Deposito, hoje);
        _transacaoService.Cadastrar(reserva.Id, 2_000m, TipoTransacao.Deposito, hoje);

        _transacaoService.Cadastrar(notebook.Id, 8_000m, TipoTransacao.Deposito, hoje);
    }
}
