# Projeto 5 — Gerenciador de Clínica

Sistema de gestão de uma clínica, desenvolvido em **4 níveis de evolução**, cada
um em um projeto .NET 8 independente. A ideia é que dê para abrir os quatro lado
a lado e ver a mesma regra de negócio migrando de um `if` dentro de um serviço
de console até um handler de CQRS com evento de domínio.

| Nível | Pasta | O que entra de novo |
|---|---|---|
| Básico I | `01-Basico-I-Console` | Console, POO, regras explícitas, tudo em memória |
| Básico II | `02-Basico-II-Api` | ASP.NET Core, EF Core InMemory, Swagger, autenticação Basic, HostedService |
| Intermediário | `03-Intermediario-API` | FluentValidation, Repository, middleware de erro, InputModel/ViewModel, `IEntityTypeConfiguration`, SQLite, JWT, objetos de valor |
| Avançado | `04-Avancado-CleanArch` | Clean Architecture em 4 projetos, CQRS com MediatR, Unit of Work com transação, eventos de domínio, testes de unidade |

---

## Funcionalidades

- **CRUD de Paciente**, com **busca por CPF** e **busca por celular**
- **CRUD de Médico**
- **CRUD de Serviço**
- **CRUD de Atendimento** (a agenda), com detecção de conflito de horário
- **PLUS 2 — Confirmação de agendamento**: e-mail com convite `.ics` anexado +
  SMS com link do Google Agenda
- **PLUS 2 — Background Service** que notifica no dia anterior
- **PLUS 2 — Autenticação e autorização** com três perfis: `Administrador`,
  `Medico`, `Recepcionista`
- **PLUS 2 — CRUD de Anexo**, nos tipos `Atestado`, `Receita` e `Evolucao`

---

## Como rodar

Precisa do **.NET 8 SDK**. Em cada pasta:

```bash
# Básico I
cd 01-Basico-I-Console/src/Clinica.Console
dotnet run

# Básico II
cd 02-Basico-II-Api/src/Clinica.Api
dotnet run          # Swagger na raiz: http://localhost:5xxx/

# Intermediário
cd 03-Intermediario-API/src/Clinica.Api
dotnet run          # cria clinica.db (SQLite) na primeira execução

# Avançado
cd 04-Avancado-CleanArch
dotnet run --project src/Clinica.Api
dotnet test         # roda os testes de unidade
```

Os quatro já sobem com dados de exemplo: 3 pacientes, 2 médicos, 3 serviços,
3 atendimentos (um deles **amanhã**, para o serviço de lembretes ter o que
notificar) e 3 usuários, um de cada perfil.

### Usuários de teste

| E-mail | Senha | Perfil |
|---|---|---|
| `admin@clinicavida.com.br` | `Clinica123` | Administrador |
| `recepcao@clinicavida.com.br` | `Clinica123` | Recepcionista |
| `helena@clinicavida.com.br` | `Clinica123` | Medico (vinculada à Dra. Helena) |

Nos níveis Intermediário e Avançado, faça `POST /api/autenticacao/login` e use o
token no botão **Authorize** do Swagger. No Básico II a autenticação é HTTP
Basic: o próprio Swagger pede e-mail e senha.

---

## As regras de negócio que valem a pena olhar

### Conflito de agenda

É a regra mais importante do sistema, e ela mora na entidade:

```csharp
public bool ConflitaCom(Atendimento outro)
    => ConflitaComIntervalo(outro.DataHora, outro.DuracaoMinutos);

public bool ConflitaComIntervalo(DateTime inicio, int duracaoMinutos)
    => DataHora < inicio.AddMinutes(duracaoMinutos) && inicio < DataHoraFim;
```

Dois intervalos colidem quando cada um começa antes do outro terminar. O caso de
borda que importa na prática é o horário **encostado**: 9:00–9:30 e 9:30–10:00
não conflitam, e é assim que se enfileira a agenda de um consultório. Há teste
para cada borda (`-30`, `-29`, `0`, `29`, `30`, `31` minutos de deslocamento).

A verificação roda para **os dois lados**: o médico não pode ter dois pacientes
no mesmo horário, e o paciente não pode estar em dois consultórios ao mesmo
tempo.

### Duração e valor são copiados no agendamento

`Atendimento` guarda a própria cópia de `DuracaoMinutos` e `Valor`, em vez de ler
do `Servico`. Sem isso, a clínica reajustar o preço da consulta mudaria o valor
de atendimentos já marcados, e aumentar a duração do serviço empurraria agendas
inteiras. Há teste provando que alterar o serviço depois não mexe no que já
estava marcado.

### Anexo só em atendimento realizado

Atestado, receita e evolução nascem durante a consulta. A entidade recusa anexo
em atendimento agendado, confirmado, cancelado ou com falta.

### Remarcar zera confirmação e lembrete

O paciente foi avisado de um horário que já não vale, então `ConfirmacaoEnviadaEm`
e `LembreteEnviadoEm` voltam a `null` e o status volta para `Agendado`. No nível
Avançado, o handler do evento `AtendimentoRemarcado` reenvia a confirmação
automaticamente.

### Nada se apaga

Toda remoção é lógica (`EstaDeletado`). Em clínica, o histórico é o registro
clínico do paciente. Do Intermediário em diante isso é um **filtro global de
consulta** no EF (`HasQueryFilter`), para que nenhuma consulta precise lembrar de
excluir os removidos — esquecer disso uma vez já é vazamento de dado removido.

Um atendimento **realizado** não pode nem ser removido logicamente.

### CPF e celular de verdade

`Documentos.CpfValido` calcula os **dígitos verificadores** (soma ponderada mod
11) e recusa sequências de dígitos iguais. O celular exige DDD existente (a
lista dos 67 DDDs válidos) e o nono dígito começando em `9`.

A busca por celular devolve **lista**, não um registro: o mesmo número costuma
atender mãe e filho. Os dados de exemplo já trazem esse caso.

### Senha

PBKDF2-SHA256, 100.000 iterações, salt de 16 bytes por senha, comparação em
tempo fixo (`CryptographicOperations.FixedTimeEquals`). Senha em texto claro não
existe em lugar nenhum, e o hash e o salt nunca saem pela API.

O login devolve a **mesma mensagem** para e-mail inexistente e senha errada —
diferenciar entregaria ao atacante quais contas existem. Há teste para isso.

### Perfis

- **Administrador**: tudo, incluindo cadastro de médicos, serviços e usuários
- **Recepcionista**: pacientes e agenda (marcar, remarcar, confirmar, cancelar)
- **Medico**: vê apenas a **própria agenda** e produz documento clínico

O filtro da agenda do médico vem do token (claim `clinica:medico_id`), e o filtro
do token **vence** o que vier na query string — senão bastaria passar
`?medicoId=outro` para ver a agenda alheia.

Não é possível desativar o último administrador ativo: o sistema ficaria sem
ninguém capaz de reativá-lo.

---

## Confirmação de agendamento e Google Agenda, sem API externa

A integração com calendário é feita gerando um arquivo **iCalendar (RFC 5545)**
e um **link de template do Google Agenda**:

- o `.ics` vai anexado ao e-mail de confirmação, e abre no Google Agenda, no
  Outlook e no Apple Calendar com um clique;
- o link `calendar.google.com/calendar/render?action=TEMPLATE&...` serve para o
  SMS, onde não dá para anexar arquivo.

O `.ics` traz um `VALARM` com `TRIGGER:-P1D` — alarme 24 horas antes, que é
exatamente a janela em que o background service dispara o lembrete. Quem aceita
o convite recebe os dois avisos no mesmo momento, por caminhos diferentes.

Nenhum pacote externo, nenhuma credencial de API, nenhuma conta Google
necessária.

### O envio em si

Não há servidor de SMTP nem gateway de SMS neste treinamento, então o envio é
gravado numa **caixa de saída** e registrado no log. A montagem da mensagem, o
`.ics` e o link são reais; só o transporte é simulado. Do Intermediário em diante
isso está atrás da interface `IEnviadorDeNotificacao`, então trocar por envio de
verdade é registrar outra implementação no `Program` — nenhum caso de uso muda.

Endpoints para ver o resultado:

```
GET  /api/notificacoes                      # caixa de saída
GET  /api/notificacoes/{id}                 # a mensagem inteira, com o corpo
GET  /api/atendimentos/{id}/agenda.ics      # baixa o convite
GET  /api/atendimentos/{id}/link-google-agenda
```

---

## Background Service

`LembreteBackgroundService` roda junto com a API e, a cada 30 minutos (vem de
`appsettings`), busca os atendimentos de **amanhã** que estão em aberto e ainda
não receberam lembrete.

Três detalhes que fazem diferença:

- o serviço é **singleton** e os serviços de aplicação são **scoped**, então cada
  rodada abre o próprio escopo com `IServiceScopeFactory`;
- cada rodada está dentro de `try/catch`, para que uma falha não derrube o laço e
  deixe a clínica sem lembretes até o próximo deploy;
- o carimbo `LembreteEnviadoEm` torna a operação **idempotente**: rodar duas
  vezes no mesmo dia não manda duas mensagens.

Para testar sem esperar:

```
POST /api/notificacoes/rodada-de-lembretes
```

Chame duas vezes seguidas — a segunda devolve zero. No Básico I isso é a opção
`6 → 2` do menu.

---

## O que muda em cada nível

### Básico I — Console

POO puro, listas em memória, validação em métodos estáticos. As entidades já têm
comportamento (`Confirmar`, `Cancelar`, `ConflitaCom`), não são sacos de
propriedades. Menu interativo com 8 seções, login com os três perfis e dados de
exemplo prontos.

### Básico II — API + EF Core InMemory

Entra ASP.NET Core, EF Core com provider InMemory e Swagger. O mapeamento está
todo no `OnModelCreating`, com `Ignore` explícito nas propriedades calculadas —
senão o EF tentaria criar coluna para `Idade` e `DataHoraFim`.

A autenticação é **HTTP Basic**, implementada com `AuthenticationHandler<>` do
próprio framework compartilhado: resolve o PLUS de autenticação e autorização com
`[Authorize(Roles = ...)]` de verdade, sem pacote externo. A API devolve as
entidades, então o `Program` precisa de `ReferenceHandler.IgnoreCycles` — problema
que desaparece no nível seguinte.

### Intermediário

- **FluentValidation** para formato, e o domínio para invariante. A divisão é:
  o que dá para checar olhando só a requisição fica no validador; o que depende
  do banco (CPF repetido, conflito de agenda) fica no serviço; o que é invariante
  da entidade fica na entidade. A vantagem de validar antes é devolver **todos**
  os erros de uma vez, agrupados por campo.
- **Repository** com interface, para a regra não depender do EF.
- **Middleware** de tratamento de erro envolvendo o pipeline: nenhum controller
  tem `try/catch`. `ValidationException` → 400 com erros por campo;
  `RegraNegocioException` → 400 com mensagem; o resto → 500 genérico, com o
  detalhe no log e não na resposta.
- **InputModel / ViewModel**: o cliente não consegue mandar `Id`, `Status` ou
  `EstaDeletado`, e a resposta não arrasta hash de senha nem bytes de anexo.
- **`IEntityTypeConfiguration`**: um arquivo de mapeamento por entidade.
- **SQLite**: os dados sobrevivem ao reinício.
- **JWT** com `JwtBearer`, chave validada na subida da aplicação.
- **Objetos de valor** (`Cpf`, `Celular`, `Email`, `Crm`): um `Cpf` só existe se
  for válido, então a verificação acontece na criação e não em cada lugar que usa
  o dado. `Crm` entra como tipo possuído (`OwnsOne`), porque número e UF andam
  juntos.

### Avançado — Clean Architecture

Quatro projetos, com as dependências apontando para dentro:

```
Clinica.Dominio          <- não referencia NADA (nenhum pacote, nenhum projeto)
Clinica.Aplicacao        <- Dominio
Clinica.Infraestrutura   <- Aplicacao, Dominio
Clinica.Api              <- as três
Clinica.Testes           <- Aplicacao, Dominio
```

O `Clinica.Dominio.csproj` **não tem um único `PackageReference`**. É o que
garante que as regras de negócio não dependem de framework.

- **CQRS com MediatR**: comando é intenção de mudar estado, query é intenção de
  ler, e cada um é um tipo com nome próprio — o que torna óbvio no log o que o
  sistema está fazendo. Dois behaviors no pipeline: log com cronômetro e
  validação, de modo que nenhum handler valida nada à mão.
- **Unit of Work**: os repositórios só marcam o que mudou; quem grava é o
  `IUnitOfWork`, uma vez por caso de uso. `ExecutarEmTransacaoAsync` envolve o
  handler inteiro numa transação do banco, com guarda de reentrância (chamada
  aninhada participa da transação em curso em vez de tentar abrir outra). É o que
  fecha a janela de corrida de duas recepcionistas marcando o mesmo horário.
- **Eventos de domínio**: `IEventoDeDominio` mora no domínio e não conhece
  MediatR; a aplicação embala cada evento em `NotificacaoDeEvento<T>` e publica.
  Os eventos são recolhidos **antes** do `SaveChanges` e publicados **depois** —
  publicar antes significaria avisar o paciente de uma confirmação que a transação
  ainda pode desfazer. O recolhimento também limpa a lista, senão um segundo
  commit no mesmo escopo republicaria tudo.

  Confirmar um atendimento é uma coisa; avisar o paciente é outra. Por isso o
  envio da confirmação está em `EnviarConfirmacaoAoConfirmar`, e não dentro do
  comando `ConfirmarAtendimento` — amanhã alguém acrescenta um aviso no WhatsApp
  sem tocar no comando.
- **HostedService** sem lógica própria: só manda o comando
  `ExecutarRodadaDeLembretes` pelo MediatR. É o mesmo comando do endpoint manual,
  então agendado e manual passam pelo mesmo caminho, log e validação incluídos.
- **Testes de unidade**: xUnit + FluentAssertions + Moq. Os handlers são testados
  com os repositórios dublados — nenhum teste abre banco, e por isso rodam em
  milissegundos. Cobrem as bordas do conflito de agenda, as transições de status,
  os objetos de valor, o hash de senha e as regras que dependem do banco.

---

## Decisões que valem explicar

**Por que o evento carrega a entidade, e não só o Id.** Num agendamento novo, o
`Id` só é atribuído no `SaveChanges`, e os eventos são publicados depois disso.
Guardando o `Id` no momento em que o evento nasce, ele valeria zero. Guardando a
entidade, o handler lê o `Id` já preenchido — e ainda evita uma segunda ida ao
banco.

**Por que a verificação de conflito vem antes de alterar o atendimento.** Na
remarcação, a entidade está rastreada pelo contexto. Mexer na data primeiro e
desfazer em caso de conflito exigiria revalidar um horário que pode já ter
passado, e a exceção do rollback mascararia a original. `ConflitaComIntervalo`
existe justamente para testar um horário candidato sem mutar nada.

**Por que `EF.Property<string>` na busca livre.** `Cpf` e `Celular` são objetos
de valor convertidos para uma coluna, e o EF não sabe traduzir
`p.Cpf.Valor.Contains(...)`. `EF.Property<string>(p, nameof(Paciente.Cpf))` lê a
coluna direto, então o `LIKE` acontece no banco e não em memória.

**Por que a carga inicial não dispara eventos.** `DadosIniciais` usa
`SaveChangesAsync` direto, e não o `IUnitOfWork`. Senão a aplicação subiria
mandando confirmação de atendimento para os pacientes de exemplo. Os eventos
acumulados são descartados no fim.

**Por que um aviso do EF está suprimido.** `PossibleIncorrectRequiredNavigation
WithQueryFilterInteractionWarning` dispara quando um dependente obrigatório e o
principal têm, os dois, filtro global de consulta — caso de `Anexo` e
`Atendimento` com a remoção lógica. O aviso está certo em geral, mas aqui o
comportamento é o desejado: anexo de atendimento removido também sai das
consultas.

---

## Premissas que assumi (o diagrama não veio no anexo)

A especificação dizia *"Veja o Diagrama em anexo"*, mas o anexo não chegou junto
com a mensagem. Então modelei a partir do texto e **listo aqui o que inventei**,
para você corrigir onde divergir do diagrama:

1. **Entidades.** `Paciente`, `Medico`, `Servico`, `Atendimento`, `Anexo` e
   `Usuario`. `Atendimento` é a entidade que liga paciente + médico + serviço +
   horário, e é dela que o `Anexo` depende.

2. **`Atendimento` guarda cópia de `DuracaoMinutos` e `Valor`.** Não vi isso
   especificado; foi decisão minha, pelo motivo explicado acima.

3. **`StatusAtendimento`**: `Agendado`, `Confirmado`, `Realizado`, `Cancelado`,
   `NaoCompareceu`. O texto não listava os status; derivei do fluxo de uma
   recepção (confirmação, comparecimento, falta).

4. **Horário de funcionamento**: 07:00 às 20:00, fechado domingo. Inventado —
   está em constantes (`Atendimento.AberturaClinica` / `FechamentoClinica`) e é
   um ajuste de uma linha.

5. **Anexo só em atendimento realizado.** Interpretação minha de atestado,
   receita e evolução serem documentos de consulta. Se a clínica precisar anexar
   pedido de exame antes da consulta, a regra está num único método
   (`Atendimento.AnexarDocumento`).

6. **`Usuario` com `MedicoId` opcional.** Foi como liguei o perfil `Medico` ao
   cadastro do médico, para o sistema saber qual agenda é dele.

7. **Busca por celular devolvendo lista.** Assumi que o mesmo número pode ser de
   mais de um paciente (mãe e filho).

8. **Anexo com os bytes na própria tabela**, limite de 10 MB. Num cenário real o
   lugar disso é um storage com a tabela guardando só a chave.

9. **`Servico` com nome único** e duração de 1 a 480 minutos.

10. **Confirmação indo nos dois canais** (e-mail e SMS) em vez de o usuário
    escolher um. Trocar para escolher o canal é mexer só no `MontadorDeMensagens`.

Se o diagrama trouxer entidade, campo ou status diferente, me manda que eu ajusto
— a maior parte disso está isolada em um ou dois arquivos por nível.
