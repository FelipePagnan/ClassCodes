
--------------funcoes matematicas--------------------------------------------------------
select id_instrutor, round(salario,1) salario_round, salario from graduacao.instrutor;
select id_instrutor, round(salario,0) salario_round, salario from graduacao.instrutor;

--------------funcoes conversão--------------------------------------------------------
select id_instrutor, cast(salario as int) salario_int, salario from graduacao.instrutor;
select nome_departamento, convert(decimal(7,1), orcamento,2) salario_decimal, orcamento from graduacao.departamento;
select id_instrutor, cast(salario as char) salario_char, salario from graduacao.instrutor;
select nome_departamento, convert(varchar, orcamento) salario_varchar, orcamento from graduacao.departamento;

--------------funcoes de cadeias de caracteres--------------------------------------------------------
select id_instrutor, upper(nome) nome_upper, salario,nome from graduacao.instrutor;
select id_instrutor, nome, salario from graduacao.instrutor where upper(nome) = 'ARIAS';
select id_aluno, nome, substring(nome,1,3) nome_substring_1, substring(nome,4,3) nome_substring_2 from graduacao.aluno;
select id_aluno, nome, replace(substring(nome,1,3),'r','m') from graduacao.aluno;
select id_aluno, nome, replace(id_aluno,'1','00001') from graduacao.aluno;

--------------funcoes de datas--------------------------------------------------------
select id_aluno, nome,sysdatetime() data_sistemica from graduacao.aluno;
select id_aluno, nome ,current_timestamp current_date_1 from graduacao.aluno;
select id_aluno, nome ,month(current_timestamp) mes from graduacao.aluno;
select id_aluno, nome ,day(current_timestamp) dia from graduacao.aluno;
select id_aluno, nome ,year(current_timestamp) ano from graduacao.aluno;
select id_aluno, nome ,datediff(year,format(current_timestamp, 'yyyy-MM-dd'),'2020-01-01') diff_date from graduacao.aluno;
select id_aluno, nome ,format(current_timestamp,'yyyy-MM-dd') format_date from graduacao.aluno;

--------------funcoes dos usuários--------------------------------------------------------
create function graduacao.qntd_aumenta_salario(@range_salario numeric(10,2))

	returns int	
	with execute as CALLER
	as
	begin

	declare @quantidade_aumentos int;

	set @quantidade_aumentos = (select count(*) from graduacao.instrutor where salario <= @range_salario)

	return @quantidade_aumentos;

	end;

select graduacao.qntd_aumenta_salario(70000.00);

------------------------------------------------------------------------------------
create function graduacao.semestre_salario(@semestre varchar(15),@ano numeric(4,0))

	returns table
	as
	return 
	(

	select a.id_professor, b.salario, b.salario * 0.02 salario_adiciona, b.nome, c.titulo from graduacao.professor a
	inner join graduacao.instrutor b  on a.id_professor = b.id_instrutor 
	inner join graduacao.curso c  on a.id_curso = c.id_curso
	where semestre = @semestre and ano = @ano);

select * from graduacao.semestre_salario('primavera',2004);

drop function graduacao.semestre_salario;


--------------------------------------procedure-------------------------------------------------
create procedure graduacao.proc_count_dept(@nome_dept varchar(20),  @count_d integer out)
	as

	begin

		select count(*)  count_d from graduacao.instrutor where instrutor.nome_departamento = @nome_dept

	end
declare @count_d int;
exec graduacao.proc_count_dept 'Physics',@count_d ;

---------------------------------------------------------------------------------------------------------------
create procedure graduacao.relatorio_aluno_semestre(@semestre varchar(20),  @ano integer)
	as

	begin

		if @semestre = 'primavera'

		select *  from graduacao.nota_aluno where nota_aluno.semestre = @semestre and nota_aluno.ano = @ano

	
		else


				select *  from graduacao.nota_aluno where nota_aluno.semestre = @semestre and nota_aluno.ano = @ano
	end

drop procedure graduacao.relatorio_aluno_semestre
exec graduacao.relatorio_aluno_semestre 'outono', 2007 ;

----------------------------------------------------------------------------------------------------------------------------

create procedure graduacao.#aumento_salario(@range_salario numeric(8,2),  @aumento decimal(4,3))
	as

	begin


		update graduacao.instrutor
		set salario = salario * @aumento
		where salario <= @range_salario 
		print 'Aumento salarial aplicado'
	end

drop procedure graduacao.#aumento_salario;
exec graduacao.#aumento_salario 60000.00, 1.2;

--------------------------------------tabelas temporárias-------------------------------------------------
create table #departamento
	(nome_departamento	varchar(50), 
	 predio				varchar(30), 
	 orcamento		    numeric(12,2) check (orcamento > 0),
	 primary key (nome_departamento)
	);


insert into dbo.#departamento values('Civil Eng.', 'Chandler', 255041.46);
insert into dbo.#departamento values('Biology', 'Candlestick', 647610.55);
insert into dbo.#departamento values('History', 'Taylor', 699140.86);
insert into dbo.#departamento values('Physics', 'Wrigley', 942162.76);
insert into dbo.#departamento values('Marketing', 'Lambeau', 210627.58);
insert into dbo.#departamento values('Pol. Sci.', 'Whitman', 573745.09);
insert into dbo.#departamento values('English', 'Palmer', 611042.66);
insert into dbo.#departamento values('Accounting', 'Saucon', 441840.92);
insert into dbo.#departamento values('Comp. Sci.', 'Lamberton', 106378.69);
insert into dbo.#departamento values('Languages', 'Linderman', 601283.60);
insert into dbo.#departamento values('Finance', 'Candlestick', 866831.75);
insert into dbo.#departamento values('Geology', 'Palmer', 406557.93);
insert into dbo.#departamento values('Cybernetics', 'Mercer', 794541.46);
insert into dbo.#departamento values('Astronomy', 'Taylor', 617253.94);
insert into dbo.#departamento values('Athletics', 'Bronfman', 734550.70);
insert into dbo.#departamento values('Statistics', 'Taylor', 395051.74);
insert into dbo.#departamento values('Psychology', 'Thompson', 848175.04);
insert into dbo.#departamento values('Math', 'Brodhead', 777605.11);
insert into dbo.#departamento values('Elec. Eng.', 'Main', 276527.61);
insert into dbo.#departamento values('Mech. Eng.', 'Rauch', 520350.65);

drop table dbo.#departamento;
--------------------------------------------------------------------------------------

create table ##sala_aula
	(predio		    varchar(15),
	 numero_sala    varchar(10),
	 capacity		numeric(3),
	 primary key (predio, numero_sala)
	);

insert into  dbo.##sala_aula values('Lamberton', 134, 10);
insert into  dbo.##sala_aula values('Chandler', 375, 10);
insert into  dbo.##sala_aula values('Fairchild', 145, 27);
insert into  dbo.##sala_aula values('Nassau', 45, 92);
insert into  dbo.##sala_aula values('Grace', 40, 34);
insert into  dbo.##sala_aula values('Whitman', 134, 120);
insert into  dbo.##sala_aula values('Lamberton', 143, 10);
insert into  dbo.##sala_aula values('Taylor', 812, 115);
insert into  dbo.##sala_aula values('Saucon', 113, 109);
insert into  dbo.##sala_aula values('Painter', 86, 97);
insert into  dbo.##sala_aula values('Alumni', 547, 26);
insert into  dbo.##sala_aula values('Alumni', 143, 47);
insert into  dbo.##sala_aula values('Drown', 757, 18);
insert into  dbo.##sala_aula values('Saucon', 180, 15);
insert into  dbo.##sala_aula values('Whitman', 434, 32);
insert into  dbo.##sala_aula values('Saucon', 844, 24);
insert into  dbo.##sala_aula values('Bronfman', 700, 12);
insert into  dbo.##sala_aula values('Polya', 808, 28);
insert into  dbo.##sala_aula values('Gates', 707, 65);
insert into  dbo.##sala_aula values('Gates', 314, 10);
insert into  dbo.##sala_aula values('Main', 45, 30);
insert into  dbo.##sala_aula values('Taylor', 183, 71);
insert into  dbo.##sala_aula values('Power', 972, 10);
insert into  dbo.##sala_aula values('Garfield', 119, 59);
insert into  dbo.##sala_aula values('Rathbone', 261, 60);
insert into  dbo.##sala_aula values('Stabler', 105, 113);
insert into  dbo.##sala_aula values('Power', 717, 12);
insert into  dbo.##sala_aula values('Main', 425, 22);
insert into  dbo.##sala_aula values('Lambeau', 348, 51);
insert into  dbo.##sala_aula values('Chandler', 804, 11);

drop table  ##sala_aula;
------------------------------------------------------------------------------
declare @grade_horario table 
	(id_horario		varchar(4),
	 dia			varchar(1),
	 inicio_hora	numeric(2) check (inicio_hora >= 0 and inicio_hora < 24),
	 inicio_min		numeric(2) check (inicio_min >= 0 and inicio_min < 60),
	 fim_hora		numeric(2) check (fim_hora >= 0 and fim_hora < 24),
	 fim_min		numeric(2) check (fim_min >= 0 and fim_min < 60),
	 primary key (id_horario, dia, inicio_hora, inicio_min)
	);

insert into @grade_horario values ( 'A', 'M', 8, 0, 8, 50);
insert into @grade_horario values ( 'A', 'W', 8, 0, 8, 50);
insert into @grade_horario values ( 'A', 'F', 8, 0, 8, 50);
insert into @grade_horario values ( 'B', 'M', 9, 0, 9, 50);
insert into @grade_horario values ( 'B', 'W', 9, 0, 9, 50);
insert into @grade_horario values ( 'B', 'F', 9, 0, 9, 50);
insert into @grade_horario values ( 'C', 'M', 11, 0, 11, 50);
insert into @grade_horario values ( 'C', 'W', 11, 0, 11, 50);
insert into @grade_horario values ( 'C', 'F', 11, 0, 11, 50);
insert into @grade_horario values ( 'D', 'M', 13, 0, 13, 50);
insert into @grade_horario values ( 'D', 'W', 13, 0, 13, 50);
insert into @grade_horario values ( 'D', 'F', 13, 0, 13, 50);
insert into @grade_horario values ( 'E', 'T', 10, 30, 11, 45);
insert into @grade_horario values ( 'E', 'R', 10, 30, 11, 45);
insert into @grade_horario values ( 'F', 'T', 14, 30, 15, 45);
insert into @grade_horario values ( 'F', 'R', 14, 30, 15, 45);
insert into @grade_horario values ( 'G', 'M', 16, 0, 16, 50);
insert into @grade_horario values ( 'G', 'W', 16, 0, 16, 50);
insert into @grade_horario values ( 'G', 'F', 16, 0, 16, 50);
insert into @grade_horario values ( 'H', 'W', 10, 0, 12, 30);

select * from @grade_horario


-----------------------Views---------------------------------------------------------

create view graduacao.relatorio_professores
as
select a.id_professor, b.salario, b.salario * 0.02 salario_adiciona, b.nome, c.titulo from graduacao.professor a
	inner join graduacao.instrutor b  on a.id_professor = b.id_instrutor 
	inner join graduacao.curso c  on a.id_curso = c.id_curso;

 