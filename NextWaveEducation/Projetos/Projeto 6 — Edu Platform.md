# Desafio Abril 2024

## Modelos de Dados

### Usuários
- **Id**: int / guid
- **Nome / Sobrenome / Nome Completo**: string
- **Email**: string
- **Senha (Criptografada)**: string
- **Data de Nascimento**: datetime
- **Documento**: string
- **Telefone Celular**: string
- **Role / Papel**: Enum (int) / String
- **Ativo**: bool

### Assinatura
- **Id**: int / guid
- **Nome**: string
- **Duração**: int
- **List\<Cursos>**

### UsuarioAssinatura
- **Id**: int / guid
- **IdUsuario**: int / guid
- **IdAssinatura**: int / guid
- **Status**: Enum (Pendente / Ativo / Desativado / Expirado)
- **Data de Início**: Datetime
- **Data de Expiração**: Datetime

### PagamentoAssinatura
- **Id**
- **Data de Processamento**
- **Status**: Sucesso / Falha / Pendente / Atrasado
- **Mensagem**: string
- **Valor**: decimal
- **IdUsuarioAssinatura**: int / Guid
- **LinkPagamento**: string
- **IdExternoPagamento**: string
- **Data de Vencimento**: DateTime

### Curso
- **Id**: int / Guid
- **Nome**: string
- **Descricao**: string
- **Cover**: string
- **Data de Criação**: Datetime

### Modulo
- **Id**: int / Guid
- **Nome**: string
- **Descricao**: string
- **Data de Criacao**: datetime

### Aula
- **Id**: int / Guid
- **Nome**: string
- **Descricao**: string
- **LinkVideo**: string
- **Duracao**: int

### UsuarioAulaConcluida
- **Id**: int / Guid
- **IdUsuario**: int / Guid
- **IdAula**: int / Guid
- **DataConclusao**: DateTime
- **Nota**: int

---

## Etapas do Desafio

### I
- CRUD Assinatura
- CRUD Curso
- CRUD Modulo
- CRUD Aula
- CRUD Usuario
- Conectar Usuario com Assinatura

### II
- Concluir Aula com Nota (UsuarioAulaConcluida)
- Status de Curso para Usuário: Quando carregar o curso para um usuário, puxar tempo total, tempo restante, porcentagem

### III
- Se inscrever em uma assinatura (Asaas)
  - Se integrar com Asaas, passando os dados do Cliente e criando o Cliente lá
  - Criar Cobrança no Asaas com o Id do Cliente
  - Obter o Id da cobrança e salvar em PagamentoAssinatura
  - Criar um webhook para receber notificações das cobranças

### IV
- Aprender sobre Hotmart e Webhooks