--comando para criar banco de dados
--create database universidade;

--comando para criar schema no banco de dados
create schema graduacao;


create table graduacao.sala_aula
	(predio		    varchar(15),
	 numero_sala    varchar(10),
	 capacity		numeric(3),
	 primary key (predio, numero_sala)
	);

create table graduacao.departamento
	(nome_departamento	varchar(50), 
	 predio				varchar(30), 
	 orcamento		    numeric(12,2) check (orcamento > 0),
	 primary key (nome_departamento)
	);


create table graduacao.curso
	(id_curso			varchar(8), 
	 titulo				varchar(50), 
	 nome_departamento	varchar(50),
	 creditos			numeric(4) check (creditos > 0),
	 primary key (id_curso),
	 foreign key (nome_departamento) references graduacao.departamento (nome_departamento)
		on delete set null
	);

create table graduacao.instrutor
	(id_instrutor		varchar(8), 
	 nome				varchar(30) not null, 
	 nome_departamento	varchar(50), 
	 salario			numeric(8,2) check (salario > 29000),
	 primary key (id_instrutor),
	 foreign key (nome_departamento) references graduacao.departamento (nome_departamento)
		on delete set null
	);

create table graduacao.secao
	(id_curso		varchar(8), 
     sec_id			varchar(8),
	 semestre		varchar(15) check (semestre in ('outono', 'inverno', 'primavera', 'verão')), 
	 ano            numeric(4) check (ano > 1701 and ano < 2100), 
	 predio		    varchar(15),
	 numero_sala    varchar(10),
	 id_slot_tempo	varchar(4),

	 primary key (id_curso, sec_id, semestre, ano),
	 foreign key (id_curso) references graduacao.curso (id_curso)
		on delete cascade,
	 foreign key (predio, numero_sala) references graduacao.sala_aula (predio, numero_sala)
		on delete set null
	);

create table graduacao.professor
	(id_professor	varchar(8), 
	 id_curso		varchar(8),
	 sec_id			varchar(8), 
	 semestre		varchar(15),
	 ano			numeric(4),
	 primary key (id_professor, id_curso, sec_id, semestre, ano),
	 foreign key (id_curso, sec_id, semestre, ano) references graduacao.secao (id_curso, sec_id, semestre, ano)
		on delete cascade,
	 foreign key (id_professor) references graduacao.instrutor (id_instrutor)
		on delete cascade
	);

create table graduacao.aluno
	(id_aluno           varchar(8), 
	 nome			    varchar(20) not null, 
	 nome_departamento	varchar(50), 
	 total_creditos		numeric(5) check (total_creditos >= 0),
	 primary key (id_aluno),
	 foreign key (nome_departamento) references graduacao.departamento (nome_departamento)
		on delete set null
	);

create table graduacao.nota_aluno
	(id_aluno	varchar(8), 
	 id_curso	varchar(8),
	 sec_id		varchar(8), 
	 semestre	varchar(15),
	 ano		numeric(4),
	 nota		varchar(2),
	 primary key (id_aluno, id_curso, sec_id, semestre, ano),
	 foreign key (id_curso, sec_id, semestre, ano) references graduacao.secao (id_curso, sec_id, semestre, ano)
		on delete cascade,
	 foreign key (id_aluno) references graduacao.aluno (id_aluno)
		on delete cascade
	);

create table graduacao.orientador_aluno
	(id_aluno		varchar(8),
	 id_orientador	varchar(8),
	 primary key (id_aluno),
	 foreign key (id_orientador) references graduacao.instrutor (id_instrutor)
		on delete set null,
	 foreign key (id_aluno) references graduacao.aluno (id_aluno)
		on delete cascade
	);

create table graduacao.grade_horario
	(id_horario		varchar(4),
	 dia			varchar(1),
	 inicio_hora	numeric(2) check (inicio_hora >= 0 and inicio_hora < 24),
	 inicio_min		numeric(2) check (inicio_min >= 0 and inicio_min < 60),
	 fim_hora		numeric(2) check (fim_hora >= 0 and fim_hora < 24),
	 fim_min		numeric(2) check (fim_min >= 0 and fim_min < 60),
	 primary key (id_horario, dia, inicio_hora, inicio_min)
	);

create table graduacao.curso_pre_requisito
	(id_curso		varchar(8), 
	 id_dependencia	varchar(8),
	 primary key (id_curso, id_dependencia)
	);

