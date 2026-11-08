using Clinica.Console.Entidades;
using Clinica.Console.Enums;
using Clinica.Console.Excecoes;
using Clinica.Console.Servicos;
using Clinica.Console.Util;
using Sys = System.Console;

namespace Clinica.Console;

/// <summary>
/// Básico I — Gerenciador de Clínica em aplicação Console.
///
/// Tudo em memória, sem banco e sem framework: o objetivo deste nível é
/// modelar o domínio com POO e deixar as regras de negócio explícitas.
/// </summary>
public static class Program
{
    private static readonly PacienteService _pacientes = new();
    private static readonly MedicoService _medicos = new();
    private static readonly ServicoService _servicos = new();
    private static readonly AtendimentoService _atendimentos =
        new(_pacientes, _medicos, _servicos);
    private static readonly UsuarioService _usuarios = new(_medicos);
    private static readonly NotificacaoService _notificacoes = new(_atendimentos);

    public static void Main()
    {
        Sys.OutputEncoding = System.Text.Encoding.UTF8;

        PopularDadosIniciais();

        Sys.WriteLine();
        Sys.WriteLine("=========================================================");
        Sys.WriteLine("        CLÍNICA VIDA — Gerenciador de Clínica");
        Sys.WriteLine("                   Básico I (Console)");
        Sys.WriteLine("=========================================================");

        if (!FazerLogin()) return;

        LoopPrincipal();
    }

    // ================================ Login ================================

    private static bool FazerLogin()
    {
        Sys.WriteLine();
        Sys.WriteLine("Usuários de teste (senha de todos: Clinica123):");

        foreach (var usuario in _usuarios.ListarTodos())
            Sys.WriteLine($"  {usuario.Email,-30} {usuario.Perfil}");

        for (var tentativa = 1; tentativa <= 3; tentativa++)
        {
            Sys.WriteLine();
            var email = Ler("E-mail");
            if (email is null) return false;

            var senha = Ler("Senha");
            if (senha is null) return false;

            try
            {
                var usuario = _usuarios.Login(email, senha);
                Sys.WriteLine();
                Sys.WriteLine($"Bem-vindo(a), {usuario.Nome}! Perfil: {usuario.Perfil}");
                return true;
            }
            catch (RegraNegocioException ex)
            {
                Sys.WriteLine($"  {ex.Message} (tentativa {tentativa} de 3)");
            }
        }

        Sys.WriteLine();
        Sys.WriteLine("Número de tentativas excedido. Encerrando.");
        return false;
    }

    // ============================ Menu principal ============================

    private static void LoopPrincipal()
    {
        while (true)
        {
            var logado = _usuarios.UsuarioLogado;
            if (logado is null) return;

            Sys.WriteLine();
            Sys.WriteLine("---------------------------------------------------------");
            Sys.WriteLine($" MENU  —  {logado.Nome} ({logado.Perfil})");
            Sys.WriteLine("---------------------------------------------------------");
            Sys.WriteLine(" 1. Pacientes");
            Sys.WriteLine(" 2. Médicos");
            Sys.WriteLine(" 3. Serviços");
            Sys.WriteLine(" 4. Atendimentos (agenda)");
            Sys.WriteLine(" 5. Anexos (atestado / receita / evolução)");
            Sys.WriteLine(" 6. Notificações (confirmação e lembretes)");
            Sys.WriteLine(" 7. Usuários e permissões");
            Sys.WriteLine(" 8. Relatórios");
            Sys.WriteLine(" 9. Trocar de usuário");
            Sys.WriteLine(" 0. Sair");

            var opcao = Ler("Opção");
            if (opcao is null) return;

            try
            {
                switch (opcao)
                {
                    case "1": MenuPacientes(); break;
                    case "2": MenuMedicos(); break;
                    case "3": MenuServicos(); break;
                    case "4": MenuAtendimentos(); break;
                    case "5": MenuAnexos(); break;
                    case "6": MenuNotificacoes(); break;
                    case "7": MenuUsuarios(); break;
                    case "8": MenuRelatorios(); break;
                    case "9":
                        _usuarios.Logout();
                        if (!FazerLogin()) return;
                        break;
                    case "0":
                        Sys.WriteLine("Até logo!");
                        return;
                    default:
                        Sys.WriteLine("Opção inválida.");
                        break;
                }
            }
            catch (RegraNegocioException ex)
            {
                Sys.WriteLine();
                Sys.WriteLine($"  [regra de negócio] {ex.Message}");
            }
        }
    }

    // ============================== Pacientes ==============================

    private static void MenuPacientes()
    {
        _usuarios.Exigir(PerfilUsuario.Administrador, PerfilUsuario.Recepcionista,
                         PerfilUsuario.Medico);

        Sys.WriteLine();
        Sys.WriteLine(" 1. Listar  2. Buscar por CPF  3. Buscar por celular  " +
                      "4. Pesquisar  5. Cadastrar  6. Remover  7. Ficha");

        var opcao = Ler("Opção");
        if (opcao is null) return;

        switch (opcao)
        {
            case "1":
                ImprimirPacientes(_pacientes.ListarTodos());
                break;

            case "2":
                var cpf = Ler("CPF");
                if (cpf is null) return;

                var porCpf = _pacientes.BuscarPorCpf(cpf);
                if (porCpf is null) Sys.WriteLine("  Nenhum paciente com esse CPF.");
                else ImprimirFichaPaciente(porCpf);
                break;

            case "3":
                var celular = Ler("Celular");
                if (celular is null) return;

                ImprimirPacientes(_pacientes.BuscarPorCelular(celular).ToList());
                break;

            case "4":
                var termo = Ler("Nome, CPF ou celular");
                if (termo is null) return;

                ImprimirPacientes(_pacientes.Pesquisar(termo).ToList());
                break;

            case "5":
                CadastrarPaciente();
                break;

            case "6":
                _usuarios.Exigir(PerfilUsuario.Administrador, PerfilUsuario.Recepcionista);

                var idRemover = LerInteiro("Id do paciente");
                if (idRemover is null) return;

                _pacientes.Remover(idRemover.Value);
                Sys.WriteLine("  Paciente removido (remoção lógica).");
                break;

            case "7":
                var idFicha = LerInteiro("Id do paciente");
                if (idFicha is null) return;

                ImprimirFichaPaciente(_pacientes.ObterPorId(idFicha.Value));
                break;

            default:
                Sys.WriteLine("Opção inválida.");
                break;
        }
    }

    private static void CadastrarPaciente()
    {
        _usuarios.Exigir(PerfilUsuario.Administrador, PerfilUsuario.Recepcionista);

        var nome = Ler("Nome completo");
        if (nome is null) return;

        var cpf = Ler("CPF");
        if (cpf is null) return;

        var celular = Ler("Celular (DDD + 9 dígitos)");
        if (celular is null) return;

        var email = Ler("E-mail (opcional)");
        if (email is null) return;

        var nascimento = LerData("Data de nascimento (dd/MM/yyyy)");
        if (nascimento is null) return;

        var sexo = LerEnum<Sexo>("Sexo");
        if (sexo is null) return;

        var paciente = _pacientes.Cadastrar(new Paciente
        {
            NomeCompleto = nome,
            Cpf = cpf,
            Celular = celular,
            Email = email,
            DataNascimento = nascimento.Value,
            Sexo = sexo.Value
        });

        Sys.WriteLine($"  Paciente cadastrado com Id {paciente.Id}: " +
                      $"{paciente.NomeCompleto} — {paciente.CpfFormatado}");

        if (paciente.EhMenorDeIdade)
            Sys.WriteLine("  Atenção: paciente menor de idade, atendimento exige responsável.");
    }

    private static void ImprimirPacientes(IReadOnlyCollection<Paciente> pacientes)
    {
        Sys.WriteLine();

        if (pacientes.Count == 0)
        {
            Sys.WriteLine("  Nenhum paciente encontrado.");
            return;
        }

        Sys.WriteLine($"  {pacientes.Count} paciente(s):");
        foreach (var paciente in pacientes.OrderBy(p => p.NomeCompleto))
            Sys.WriteLine($"  {paciente}");
    }

    private static void ImprimirFichaPaciente(Paciente paciente)
    {
        Sys.WriteLine();
        Sys.WriteLine($"  Paciente ..... [{paciente.Id}] {paciente.NomeCompleto}");
        Sys.WriteLine($"  CPF .......... {paciente.CpfFormatado}");
        Sys.WriteLine($"  Celular ...... {paciente.CelularFormatado}");
        Sys.WriteLine($"  E-mail ....... {paciente.Email ?? "-"}");
        Sys.WriteLine($"  Nascimento ... {paciente.DataNascimento:dd/MM/yyyy} " +
                      $"({paciente.Idade} anos){(paciente.EhMenorDeIdade ? "  [menor]" : "")}");
        Sys.WriteLine($"  Sexo ......... {paciente.Sexo}");

        var historico = _atendimentos.ListarPorPaciente(paciente.Id).ToList();

        Sys.WriteLine();
        Sys.WriteLine($"  Histórico de atendimentos ({historico.Count}):");

        if (historico.Count == 0)
            Sys.WriteLine("    nenhum atendimento registrado.");

        foreach (var atendimento in historico)
        {
            Sys.WriteLine($"    {atendimento}");

            foreach (var anexo in atendimento.AnexosAtivos)
                Sys.WriteLine($"        anexo: {anexo}");
        }
    }

    // =============================== Médicos ===============================

    private static void MenuMedicos()
    {
        Sys.WriteLine();
        Sys.WriteLine(" 1. Listar  2. Buscar por CRM  3. Buscar por especialidade  " +
                      "4. Cadastrar  5. Remover");

        var opcao = Ler("Opção");
        if (opcao is null) return;

        switch (opcao)
        {
            case "1":
                Sys.WriteLine();
                foreach (var medico in _medicos.ListarTodos().OrderBy(m => m.NomeCompleto))
                    Sys.WriteLine($"  {medico}");
                break;

            case "2":
                var crm = Ler("CRM");
                if (crm is null) return;

                var uf = Ler("UF");
                if (uf is null) return;

                var encontrado = _medicos.BuscarPorCrm(crm, uf);
                Sys.WriteLine(encontrado is null
                    ? "  Nenhum médico com esse CRM/UF."
                    : $"  {encontrado}");
                break;

            case "3":
                var especialidade = Ler("Especialidade");
                if (especialidade is null) return;

                Sys.WriteLine();
                foreach (var medico in _medicos.BuscarPorEspecialidade(especialidade))
                    Sys.WriteLine($"  {medico}");
                break;

            case "4":
                CadastrarMedico();
                break;

            case "5":
                _usuarios.Exigir(PerfilUsuario.Administrador);

                var id = LerInteiro("Id do médico");
                if (id is null) return;

                _medicos.Remover(id.Value);
                Sys.WriteLine("  Médico removido (remoção lógica).");
                break;

            default:
                Sys.WriteLine("Opção inválida.");
                break;
        }
    }

    private static void CadastrarMedico()
    {
        _usuarios.Exigir(PerfilUsuario.Administrador);

        var nome = Ler("Nome completo");
        if (nome is null) return;

        var crm = Ler("CRM (4 a 7 dígitos)");
        if (crm is null) return;

        var uf = Ler("UF do CRM");
        if (uf is null) return;

        var especialidade = Ler("Especialidade");
        if (especialidade is null) return;

        var email = Ler("E-mail (opcional)");
        if (email is null) return;

        var medico = _medicos.Cadastrar(new Medico
        {
            NomeCompleto = nome,
            Crm = crm,
            UfCrm = uf,
            Especialidade = especialidade,
            Email = email
        });

        Sys.WriteLine($"  Médico cadastrado com Id {medico.Id}: " +
                      $"{medico.NomeCompleto} — {medico.CrmCompleto}");
    }

    // =============================== Serviços ===============================

    private static void MenuServicos()
    {
        Sys.WriteLine();
        Sys.WriteLine(" 1. Listar  2. Cadastrar  3. Atualizar  4. Remover");

        var opcao = Ler("Opção");
        if (opcao is null) return;

        switch (opcao)
        {
            case "1":
                Sys.WriteLine();
                foreach (var servico in _servicos.ListarTodos().OrderBy(s => s.Nome))
                    Sys.WriteLine($"  {servico}");
                break;

            case "2":
                _usuarios.Exigir(PerfilUsuario.Administrador);

                var nome = Ler("Nome do serviço");
                if (nome is null) return;

                var valor = LerDecimal("Valor");
                if (valor is null) return;

                var duracao = LerInteiro("Duração em minutos");
                if (duracao is null) return;

                var novo = _servicos.Cadastrar(new Servico
                {
                    Nome = nome,
                    Valor = valor.Value,
                    DuracaoMinutos = duracao.Value
                });

                Sys.WriteLine($"  Serviço cadastrado com Id {novo.Id}.");
                break;

            case "3":
                _usuarios.Exigir(PerfilUsuario.Administrador);

                var idAtualizar = LerInteiro("Id do serviço");
                if (idAtualizar is null) return;

                var atual = _servicos.ObterPorId(idAtualizar.Value);

                var novoValor = LerDecimal($"Novo valor (atual: {atual.Valor:N2})");
                if (novoValor is null) return;

                _servicos.Atualizar(atual.Id, new Servico
                {
                    Nome = atual.Nome,
                    Descricao = atual.Descricao,
                    Valor = novoValor.Value,
                    DuracaoMinutos = atual.DuracaoMinutos
                });

                Sys.WriteLine("  Serviço atualizado. Atendimentos já marcados mantêm o valor antigo.");
                break;

            case "4":
                _usuarios.Exigir(PerfilUsuario.Administrador);

                var idRemover = LerInteiro("Id do serviço");
                if (idRemover is null) return;

                _servicos.Remover(idRemover.Value);
                Sys.WriteLine("  Serviço removido (remoção lógica).");
                break;

            default:
                Sys.WriteLine("Opção inválida.");
                break;
        }
    }

    // ============================= Atendimentos =============================

    private static void MenuAtendimentos()
    {
        Sys.WriteLine();
        Sys.WriteLine(" 1. Agenda do dia  2. Listar todos  3. Agendar  4. Remarcar");
        Sys.WriteLine(" 5. Confirmar  6. Marcar realizado  7. Cancelar  8. Marcar falta");

        var opcao = Ler("Opção");
        if (opcao is null) return;

        switch (opcao)
        {
            case "1":
                var dia = LerData("Dia (dd/MM/yyyy)");
                if (dia is null) return;

                ImprimirAgenda(dia.Value);
                break;

            case "2":
                Sys.WriteLine();
                var lista = _usuarios.FiltrarPorPermissao(_atendimentos.ListarTodos()).ToList();

                if (lista.Count == 0) Sys.WriteLine("  Nenhum atendimento.");

                foreach (var atendimento in lista)
                    Sys.WriteLine($"  {atendimento}");
                break;

            case "3":
                AgendarAtendimento();
                break;

            case "4":
                _usuarios.Exigir(PerfilUsuario.Administrador, PerfilUsuario.Recepcionista);

                var idRemarcar = LerInteiro("Id do atendimento");
                if (idRemarcar is null) return;

                var novaData = LerDataHora("Nova data e hora (dd/MM/yyyy HH:mm)");
                if (novaData is null) return;

                var remarcado = _atendimentos.Remarcar(idRemarcar.Value, novaData.Value);
                Sys.WriteLine($"  Remarcado para {remarcado.DataHora:dd/MM/yyyy HH:mm}. " +
                              "A confirmação precisa ser enviada novamente.");
                break;

            case "5":
                var idConfirmar = LerInteiro("Id do atendimento");
                if (idConfirmar is null) return;

                var enviadas = _notificacoes.EnviarConfirmacao(idConfirmar.Value);
                Sys.WriteLine($"  Atendimento confirmado e {enviadas.Count} notificação(ões) enviada(s).");

                foreach (var notificacao in enviadas)
                    Sys.WriteLine($"    {notificacao.Canal} -> {notificacao.Destinatario}");
                break;

            case "6":
                var idRealizado = LerInteiro("Id do atendimento");
                if (idRealizado is null) return;

                _atendimentos.MarcarComoRealizado(idRealizado.Value);
                Sys.WriteLine("  Atendimento marcado como realizado. Já aceita anexos.");
                break;

            case "7":
                var idCancelar = LerInteiro("Id do atendimento");
                if (idCancelar is null) return;

                var motivo = Ler("Motivo (opcional)");
                if (motivo is null) return;

                _atendimentos.Cancelar(idCancelar.Value, motivo);
                Sys.WriteLine("  Atendimento cancelado.");
                break;

            case "8":
                var idFalta = LerInteiro("Id do atendimento");
                if (idFalta is null) return;

                _atendimentos.MarcarComoFalta(idFalta.Value);
                Sys.WriteLine("  Falta registrada.");
                break;

            default:
                Sys.WriteLine("Opção inválida.");
                break;
        }
    }

    private static void AgendarAtendimento()
    {
        _usuarios.Exigir(PerfilUsuario.Administrador, PerfilUsuario.Recepcionista);

        var idPaciente = LerInteiro("Id do paciente");
        if (idPaciente is null) return;

        var idMedico = LerInteiro("Id do médico");
        if (idMedico is null) return;

        var idServico = LerInteiro("Id do serviço");
        if (idServico is null) return;

        var dataHora = LerDataHora("Data e hora (dd/MM/yyyy HH:mm)");
        if (dataHora is null) return;

        var observacoes = Ler("Observações (opcional)");
        if (observacoes is null) return;

        var atendimento = _atendimentos.Agendar(
            idPaciente.Value, idMedico.Value, idServico.Value, dataHora.Value, observacoes);

        Sys.WriteLine($"  Atendimento {atendimento.Id} agendado para " +
                      $"{atendimento.DataHora:dd/MM/yyyy HH:mm} " +
                      $"({atendimento.DuracaoMinutos}min, {atendimento.Valor:N2}).");
    }

    private static void ImprimirAgenda(DateTime dia)
    {
        var usuario = _usuarios.UsuarioLogado!;

        var medicoId = usuario.Perfil == PerfilUsuario.Medico ? usuario.MedicoId : null;
        var agenda = _atendimentos.GerarAgendaDoDia(dia, medicoId);

        Sys.WriteLine();
        Sys.WriteLine($"  AGENDA DE {agenda.Dia:dd/MM/yyyy}");
        Sys.WriteLine($"  {agenda.Total} atendimento(s) | confirmados: {agenda.Confirmados} | " +
                      $"agendados: {agenda.Agendados} | realizados: {agenda.Realizados} | " +
                      $"cancelados: {agenda.Cancelados} | faltas: {agenda.Faltas}");
        Sys.WriteLine($"  Valor previsto: {agenda.ValorPrevisto:N2}");
        Sys.WriteLine();

        if (agenda.Atendimentos.Count == 0)
            Sys.WriteLine("    nada marcado para este dia.");

        foreach (var atendimento in agenda.Atendimentos)
            Sys.WriteLine($"    {atendimento}");
    }

    // ================================ Anexos ================================

    private static void MenuAnexos()
    {
        Sys.WriteLine();
        Sys.WriteLine(" 1. Listar do atendimento  2. Listar do paciente  " +
                      "3. Adicionar  4. Ver conteúdo  5. Remover");

        var opcao = Ler("Opção");
        if (opcao is null) return;

        switch (opcao)
        {
            case "1":
                var idAtendimento = LerInteiro("Id do atendimento");
                if (idAtendimento is null) return;

                Sys.WriteLine();
                var anexos = _atendimentos.ListarAnexos(idAtendimento.Value).ToList();

                if (anexos.Count == 0) Sys.WriteLine("  Nenhum anexo.");

                foreach (var anexo in anexos)
                    Sys.WriteLine($"  {anexo}");
                break;

            case "2":
                var idPaciente = LerInteiro("Id do paciente");
                if (idPaciente is null) return;

                Sys.WriteLine();
                foreach (var anexo in _atendimentos.ListarAnexosDoPaciente(idPaciente.Value))
                    Sys.WriteLine($"  atendimento {anexo.AtendimentoId}: {anexo}");
                break;

            case "3":
                AdicionarAnexo();
                break;

            case "4":
                var idAtendimentoVer = LerInteiro("Id do atendimento");
                if (idAtendimentoVer is null) return;

                var idAnexo = LerInteiro("Id do anexo");
                if (idAnexo is null) return;

                var encontrado = _atendimentos.ObterAnexo(idAtendimentoVer.Value, idAnexo.Value);

                Sys.WriteLine();
                Sys.WriteLine($"  {encontrado.Tipo} — {encontrado.NomeArquivo} " +
                              $"({encontrado.TipoConteudo}, {encontrado.TamanhoLegivel})");
                Sys.WriteLine("  ---");
                Sys.WriteLine(encontrado.TipoConteudo.StartsWith("text/")
                    ? System.Text.Encoding.UTF8.GetString(encontrado.Conteudo)
                    : "  [conteúdo binário]");
                break;

            case "5":
                _usuarios.Exigir(PerfilUsuario.Administrador, PerfilUsuario.Medico);

                var idAtendimentoRemover = LerInteiro("Id do atendimento");
                if (idAtendimentoRemover is null) return;

                var idAnexoRemover = LerInteiro("Id do anexo");
                if (idAnexoRemover is null) return;

                _atendimentos.RemoverAnexo(idAtendimentoRemover.Value, idAnexoRemover.Value);
                Sys.WriteLine("  Anexo removido (remoção lógica: documento clínico não se apaga).");
                break;

            default:
                Sys.WriteLine("Opção inválida.");
                break;
        }
    }

    private static void AdicionarAnexo()
    {
        // Só o médico produz atestado, receita e evolução.
        _usuarios.Exigir(PerfilUsuario.Medico, PerfilUsuario.Administrador);

        var idAtendimento = LerInteiro("Id do atendimento");
        if (idAtendimento is null) return;

        var tipo = LerEnum<TipoAnexo>("Tipo");
        if (tipo is null) return;

        var nomeArquivo = Ler("Nome do arquivo (ex: atestado.txt)");
        if (nomeArquivo is null) return;

        var texto = Ler("Conteúdo do documento");
        if (texto is null) return;

        var anexo = _atendimentos.AdicionarAnexoDeTexto(
            idAtendimento.Value, tipo.Value, nomeArquivo, texto);

        Sys.WriteLine($"  Anexo {anexo.Id} adicionado: {anexo.Tipo} ({anexo.TamanhoLegivel}).");
    }

    // ============================= Notificações =============================

    private static void MenuNotificacoes()
    {
        Sys.WriteLine();
        Sys.WriteLine(" 1. Enviar confirmação  2. Rodar lembretes do dia anterior");
        Sys.WriteLine(" 3. Caixa de saída  4. Ver mensagem  5. Ver convite .ics");

        var opcao = Ler("Opção");
        if (opcao is null) return;

        switch (opcao)
        {
            case "1":
                var id = LerInteiro("Id do atendimento");
                if (id is null) return;

                foreach (var notificacao in _notificacoes.EnviarConfirmacao(id.Value))
                    Sys.WriteLine($"  enviado por {notificacao.Canal} para {notificacao.Destinatario}");
                break;

            case "2":
                var rodada = _notificacoes.ExecutarRodadaDeLembretes();

                Sys.WriteLine();
                Sys.WriteLine($"  Rodada de {rodada.ExecutadaEm:dd/MM/yyyy HH:mm}");
                Sys.WriteLine($"  Atendimentos de amanhã sem lembrete: " +
                              $"{rodada.AtendimentosEncontrados}");
                Sys.WriteLine($"  Notificações enviadas: {rodada.Enviadas.Count}");

                foreach (var notificacao in rodada.Enviadas)
                    Sys.WriteLine($"    {notificacao.Canal,-5} -> {notificacao.Destinatario}  " +
                                  $"| {notificacao.Assunto}");
                break;

            case "3":
                Sys.WriteLine();
                var caixa = _notificacoes.CaixaDeSaida();

                if (caixa.Count == 0) Sys.WriteLine("  Caixa de saída vazia.");

                foreach (var notificacao in caixa)
                    Sys.WriteLine($"  [{notificacao.Id}] {notificacao.EnviadaEm:dd/MM HH:mm} " +
                                  $"{notificacao.Canal,-5} -> {notificacao.Destinatario,-24} " +
                                  $"| {notificacao.Assunto}");
                break;

            case "4":
                var idMensagem = LerInteiro("Id da notificação");
                if (idMensagem is null) return;

                var mensagem = _notificacoes.CaixaDeSaida()
                    .FirstOrDefault(n => n.Id == idMensagem.Value);

                if (mensagem is null)
                {
                    Sys.WriteLine("  Notificação não encontrada.");
                    break;
                }

                Sys.WriteLine();
                Sys.WriteLine($"  Para: {mensagem.Destinatario} ({mensagem.Canal})");
                Sys.WriteLine($"  Assunto: {mensagem.Assunto}");
                Sys.WriteLine("  ---");
                Sys.WriteLine(mensagem.Corpo);
                break;

            case "5":
                var idAtendimento = LerInteiro("Id do atendimento");
                if (idAtendimento is null) return;

                var atendimento = _atendimentos.ObterPorId(idAtendimento.Value);

                Sys.WriteLine();
                Sys.WriteLine(CalendarioIcs.Gerar(atendimento));
                Sys.WriteLine($"  Link do Google Agenda:");
                Sys.WriteLine($"  {CalendarioIcs.GerarLinkGoogleAgenda(atendimento)}");
                break;

            default:
                Sys.WriteLine("Opção inválida.");
                break;
        }
    }

    // =============================== Usuários ===============================

    private static void MenuUsuarios()
    {
        _usuarios.Exigir(PerfilUsuario.Administrador);

        Sys.WriteLine();
        Sys.WriteLine(" 1. Listar  2. Cadastrar  3. Desativar  4. Reativar");

        var opcao = Ler("Opção");
        if (opcao is null) return;

        switch (opcao)
        {
            case "1":
                Sys.WriteLine();
                foreach (var usuario in _usuarios.ListarTodos())
                    Sys.WriteLine($"  {usuario}");
                break;

            case "2":
                var nome = Ler("Nome");
                if (nome is null) return;

                var email = Ler("E-mail");
                if (email is null) return;

                var senha = Ler("Senha (8+ caracteres, com letra e número)");
                if (senha is null) return;

                var perfil = LerEnum<PerfilUsuario>("Perfil");
                if (perfil is null) return;

                int? medicoId = null;
                if (perfil == PerfilUsuario.Medico)
                {
                    medicoId = LerInteiro("Id do médico vinculado");
                    if (medicoId is null) return;
                }

                var novo = _usuarios.Cadastrar(nome, email, senha, perfil.Value, medicoId);
                Sys.WriteLine($"  Usuário {novo.Id} cadastrado como {novo.Perfil}.");
                break;

            case "3":
                var idDesativar = LerInteiro("Id do usuário");
                if (idDesativar is null) return;

                _usuarios.Desativar(idDesativar.Value);
                Sys.WriteLine("  Usuário desativado.");
                break;

            case "4":
                var idReativar = LerInteiro("Id do usuário");
                if (idReativar is null) return;

                _usuarios.Reativar(idReativar.Value);
                Sys.WriteLine("  Usuário reativado.");
                break;

            default:
                Sys.WriteLine("Opção inválida.");
                break;
        }
    }

    // ============================== Relatórios ==============================

    private static void MenuRelatorios()
    {
        Sys.WriteLine();
        Sys.WriteLine(" 1. Agenda de hoje  2. Agenda de amanhã  3. Resumo por médico");
        Sys.WriteLine(" 4. Resumo por serviço  5. Faltas e cancelamentos");

        var opcao = Ler("Opção");
        if (opcao is null) return;

        switch (opcao)
        {
            case "1":
                ImprimirAgenda(DateTime.Today);
                break;

            case "2":
                ImprimirAgenda(DateTime.Today.AddDays(1));
                break;

            case "3":
                Sys.WriteLine();
                Sys.WriteLine("  RESUMO POR MÉDICO");

                foreach (var medico in _medicos.ListarTodos())
                {
                    var atendimentos = _atendimentos.ListarPorMedico(medico.Id).ToList();

                    Sys.WriteLine($"  {medico.NomeCompleto,-24} {medico.Especialidade,-18} " +
                                  $"total: {atendimentos.Count,3} | " +
                                  $"abertos: {atendimentos.Count(a => a.EstaAberto),3} | " +
                                  $"realizados: {atendimentos.Count(a => a.Status == StatusAtendimento.Realizado),3}");
                }
                break;

            case "4":
                Sys.WriteLine();
                Sys.WriteLine("  RESUMO POR SERVIÇO");

                foreach (var servico in _servicos.ListarTodos())
                {
                    var atendimentos = _atendimentos.ListarTodos()
                        .Where(a => a.ServicoId == servico.Id).ToList();

                    var faturamento = atendimentos
                        .Where(a => a.Status == StatusAtendimento.Realizado)
                        .Sum(a => a.Valor);

                    Sys.WriteLine($"  {servico.Nome,-26} {atendimentos.Count,3} atendimento(s) | " +
                                  $"faturado: {faturamento,10:N2}");
                }
                break;

            case "5":
                Sys.WriteLine();
                var faltas = _atendimentos.ListarPorStatus(StatusAtendimento.NaoCompareceu).ToList();
                var cancelados = _atendimentos.ListarPorStatus(StatusAtendimento.Cancelado).ToList();

                Sys.WriteLine($"  Faltas: {faltas.Count} | Cancelamentos: {cancelados.Count}");

                foreach (var atendimento in faltas.Concat(cancelados).OrderBy(a => a.DataHora))
                    Sys.WriteLine($"  {atendimento}");
                break;

            default:
                Sys.WriteLine("Opção inválida.");
                break;
        }
    }

    // =========================== Entrada de dados ===========================

    /// <summary>
    /// Lê uma linha. Devolve null quando a entrada termina (stdin fechado),
    /// o que encerra o menu em vez de girar em laço infinito.
    /// </summary>
    private static string? Ler(string rotulo)
    {
        Sys.Write($"  {rotulo}: ");
        return Sys.ReadLine()?.Trim();
    }

    private static int? LerInteiro(string rotulo)
    {
        while (true)
        {
            var texto = Ler(rotulo);
            if (texto is null) return null;

            if (int.TryParse(texto, out var valor)) return valor;

            Sys.WriteLine("    Informe um número inteiro.");
        }
    }

    private static decimal? LerDecimal(string rotulo)
    {
        while (true)
        {
            var texto = Ler(rotulo);
            if (texto is null) return null;

            if (decimal.TryParse(texto.Replace(',', '.'),
                    System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var valor))
                return valor;

            Sys.WriteLine("    Informe um valor numérico, ex: 150.00");
        }
    }

    private static DateTime? LerData(string rotulo)
    {
        while (true)
        {
            var texto = Ler(rotulo);
            if (texto is null) return null;

            if (DateTime.TryParseExact(texto, "dd/MM/yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var data))
                return data;

            Sys.WriteLine("    Formato esperado: dd/MM/yyyy");
        }
    }

    private static DateTime? LerDataHora(string rotulo)
    {
        while (true)
        {
            var texto = Ler(rotulo);
            if (texto is null) return null;

            if (DateTime.TryParseExact(texto, "dd/MM/yyyy HH:mm",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var data))
                return data;

            Sys.WriteLine("    Formato esperado: dd/MM/yyyy HH:mm");
        }
    }

    private static TEnum? LerEnum<TEnum>(string rotulo) where TEnum : struct, Enum
    {
        var opcoes = Enum.GetValues<TEnum>()
            .Select(v => $"{Convert.ToInt32(v)}={v}");

        Sys.WriteLine($"    ({string.Join("  ", opcoes)})");

        while (true)
        {
            var texto = Ler(rotulo);
            if (texto is null) return null;

            if (Enum.TryParse<TEnum>(texto, ignoreCase: true, out var valor)
                && Enum.IsDefined(valor))
                return valor;

            Sys.WriteLine("    Opção inválida.");
        }
    }

    // ============================= Dados iniciais =============================

    private static void PopularDadosIniciais()
    {
        // Serviços
        var consulta = _servicos.Cadastrar(new Servico
        {
            Nome = "Consulta clínica",
            Descricao = "Consulta eletiva em consultório",
            Valor = 250.00m,
            DuracaoMinutos = 30
        });

        var retorno = _servicos.Cadastrar(new Servico
        {
            Nome = "Retorno",
            Descricao = "Revisão de exames e conduta",
            Valor = 120.00m,
            DuracaoMinutos = 20
        });

        _servicos.Cadastrar(new Servico
        {
            Nome = "Eletrocardiograma",
            Descricao = "ECG de repouso com laudo",
            Valor = 180.00m,
            DuracaoMinutos = 25
        });

        // Médicos
        var cardiologista = _medicos.Cadastrar(new Medico
        {
            NomeCompleto = "Helena Vasconcelos",
            Crm = "123456",
            UfCrm = "SP",
            Especialidade = "Cardiologia",
            Email = "helena.vasconcelos@clinicavida.com.br"
        });

        var dermatologista = _medicos.Cadastrar(new Medico
        {
            NomeCompleto = "Rafael Queiroz",
            Crm = "98765",
            UfCrm = "SP",
            Especialidade = "Dermatologia",
            Email = "rafael.queiroz@clinicavida.com.br"
        });

        // Pacientes (CPFs com dígitos verificadores válidos)
        var mariana = _pacientes.Cadastrar(new Paciente
        {
            NomeCompleto = "Mariana Lopes Ferreira",
            Cpf = "52998224725",
            Celular = "11987654321",
            Email = "mariana.ferreira@exemplo.com.br",
            DataNascimento = new DateTime(1988, 4, 17),
            Sexo = Sexo.Feminino
        });

        var bruno = _pacientes.Cadastrar(new Paciente
        {
            NomeCompleto = "Bruno Tavares Alencar",
            Cpf = "11144477735",
            Celular = "11912345678",
            Email = "bruno.alencar@exemplo.com.br",
            DataNascimento = new DateTime(1975, 11, 2),
            Sexo = Sexo.Masculino
        });

        _pacientes.Cadastrar(new Paciente
        {
            NomeCompleto = "Lívia Tavares Alencar",
            Cpf = "16899535009",
            // Mesmo celular do pai: a busca por celular devolve os dois.
            Celular = "11912345678",
            DataNascimento = DateTime.Today.AddYears(-9),
            Sexo = Sexo.Feminino,
            Observacoes = "Menor de idade. Responsável: Bruno Tavares Alencar."
        });

        // Usuários dos três perfis
        _usuarios.Cadastrar("Administração", "admin@clinicavida.com.br",
            "Clinica123", PerfilUsuario.Administrador);

        _usuarios.Cadastrar("Recepção", "recepcao@clinicavida.com.br",
            "Clinica123", PerfilUsuario.Recepcionista);

        _usuarios.Cadastrar("Helena Vasconcelos", "helena@clinicavida.com.br",
            "Clinica123", PerfilUsuario.Medico, cardiologista.Id);

        // Agenda de exemplo: amanhã (para o lembrete ter o que notificar)
        // e depois de amanhã, sempre em dia útil e dentro do horário.
        var amanha = ProximoDiaUtil(DateTime.Today.AddDays(1)).AddHours(9);
        var depois = ProximoDiaUtil(DateTime.Today.AddDays(2)).AddHours(14);

        _atendimentos.Agendar(mariana.Id, cardiologista.Id, consulta.Id, amanha,
            "Primeira consulta. Levar exames anteriores.");

        _atendimentos.Agendar(bruno.Id, cardiologista.Id, retorno.Id,
            amanha.AddMinutes(consulta.DuracaoMinutos));

        _atendimentos.Agendar(mariana.Id, dermatologista.Id, consulta.Id, depois);
    }

    /// <summary>Pula o domingo, já que a clínica não atende nesse dia.</summary>
    private static DateTime ProximoDiaUtil(DateTime data)
        => data.DayOfWeek == DayOfWeek.Sunday ? data.AddDays(1) : data;
}
