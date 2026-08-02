insert into graduacao.curso (id_curso, titulo, nome_departamento, creditos) values ('100', 'Programming', 'Comp. Sci.', 4)

insert into graduacao.instrutor
	select id_aluno, nome, nome_departamento, 36000 as salario
	from graduacao.aluno 
	where nome_departamento = 'Civil Eng.';


update graduacao.instrutor
	set salario = salario * 1.05;

update graduacao.instrutor
	set salario = salario* 1.05
	where salario< 70000;

update graduacao.instrutor
	set salario = salario* 1.05
	where salario < (select avg (salario)
	from graduacao.instrutor);

select * from graduacao.instrutor;

update graduacao.aluno
	set total_creditos = (select sum(creditos) 
	from graduacao.nota_aluno, graduacao.curso
	where nota_aluno.id_curso = curso.id_curso and 
	aluno.id_aluno = nota_aluno.id_aluno 
	and nota_aluno.nota <> 'F' 
	and nota_aluno.nota is not null);

select * from graduacao.aluno;


update graduacao.instrutor
set salario = case
when salario <= 100000 then salario * 1.05
else salario * 1.03
end;

select * from graduacao.instrutor;



delete from graduacao.professor; 

delete from graduacao.professor where ano = 2007 and id_professor = '14365';

select * from graduacao.professor where ano = 2007 and  id_professor = '14365';