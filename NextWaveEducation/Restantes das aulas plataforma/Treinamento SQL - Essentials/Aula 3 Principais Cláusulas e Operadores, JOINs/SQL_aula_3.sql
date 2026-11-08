------------------------- operacoes matematicas basicas------------------------------------
select id_instrutor, salario * 12 as salarial_anual from graduacao.instrutor;
select id_instrutor, salario + 1500 as aumento_salarial from graduacao.instrutor;
select id_instrutor, salario - 7000 as subtracao_salarial from graduacao.instrutor;
select id_instrutor, salario / 3 as ferias_adicional__salarial from graduacao.instrutor;
select id_instrutor, salario % 3 as imposto_salarial from graduacao.instrutor;

------------------------- distinct------------------------------------
select *  from graduacao.nota_aluno where id_aluno = '13403';
select distinct id_aluno, ano from graduacao.nota_aluno where id_aluno = '13403';
select *distinct id_aluno from graduacao.nota_aluno;
select distinct id_aluno, ano from graduacao.nota_aluno where id_aluno = '13403';
select  distinct id_aluno from graduacao.nota_aluno;

-------------------------case------------------------------------
select i.*,
case
when salario < 40000 then 'professor substituto'
when salario <= 70000 then 'professor pleno'
when salario < 100000 then 'professor sênior'
else 'professor adjunto'
end cargo_instrutor
from graduacao.instrutor i;


select 
j.*,
concat(cargo_instrutor, ' - ', nome_departamento) as cargo_departamento
from(
select i.*,
case
	when salario < 40000 then 'professor substituto'
	when salario <= 70000 then 'professor pleno'
	when salario < 100000 then 'professor sênior'
	else 'professor adjunto'
end cargo_instrutor
from graduacao.instrutor i) j;


select 
j.*,
cargo_instrutor + ' - ' + nome_departamento as cargo_departamento
from(
select i.*,
case
	when salario < 40000 then 'professor substituto'
	when salario <= 70000 then 'professor pleno'
	when salario < 100000 then 'professor sênior'
	else 'professor adjunto'
end cargo_instrutor
from graduacao.instrutor i) j;

-------------------------Order by------------------------------------

select * from graduacao.nota_aluno where id_aluno = '13403' order by ano asc;
select * from graduacao.nota_aluno order by id_aluno desc, ano asc;


-------------------------Agrupamento e agregações------------------------------------
select avg(total_creditos) from graduacao.aluno;
select max(total_creditos) from graduacao.aluno;
select min(total_creditos) from graduacao.aluno;
select count(id_aluno) from graduacao.nota_aluno;

select count(distinct id_aluno) from graduacao.nota_aluno;


select sum(total_creditos) from graduacao.aluno where nome_departamento = 'Pol. Sci.';


select nome_departamento, avg (salario) as media_salario
from graduacao.instrutor
group by nome_departamento;


select nome_departamento, avg (salario) as media_salario
from graduacao.instrutor
group by nome_departamento
having avg (salario) > 60000;


-------------------------Oepradores lógicos------------------------------------
select * from graduacao.nota_aluno where id_aluno = '13403';
select * from graduacao.nota_aluno where id_aluno != '13403';
select * from graduacao.nota_aluno where id_aluno <> '13403';
select * from graduacao.instrutor where salario > 42000;
select * from graduacao.aluno where total_creditos > 40;
select * from graduacao.instrutor where salario < 42000;
select * from graduacao.aluno where total_creditos < 40;
select * from graduacao.professor where ano >= 2002;
select * from graduacao.professor where ano <= 2005;


select * from graduacao.professor where ano = 2005 and semestre = 'primavera';
select * from graduacao.professor where ano = 2005 and semestre = 'verão';


select * from graduacao.professor where ano = 2005 or semestre = 'primavera';
select * from graduacao.professor where ano = 2005 or semestre = 'verão';
select * from graduacao.professor where ano = 2021 or semestre = 'verão';


select * from graduacao.aluno where total_creditos between  40 and 60 and nome_departamento = 'Civil Eng.';
select * from graduacao.aluno where total_creditos not between  40 and 60;
select * from graduacao.aluno where total_creditos not between  40 and 60 and nome_departamento = 'Civil Eng.';
select * from graduacao.aluno where total_creditos  between  1000 and 6000 or nome_departamento = 'Civil Eng.';

select * from graduacao.aluno where total_creditos in (select  total_creditos from graduacao.aluno where total_creditos > 100);

select * from graduacao.aluno a where  exists (select d.nome_departamento from graduacao.departamento d where d.nome_departamento = a.nome_departamento);


select * from graduacao.aluno where total_creditos IS NOT NULL;
select * from graduacao.aluno where total_creditos IS NULL;

select * from graduacao.aluno where nome_departamento like '%Eng.';
select * from graduacao.aluno where nome_departamento like 'Pol%';
select * from graduacao.aluno where nome_departamento like '%olo%';
select * from graduacao.aluno where nome_departamento not like '%olo%' and total_creditos >= 50;

-------------------------Inner join------------------------------------
select A.*, B.nota,B.semestre  from graduacao.aluno 
A inner join graduacao.nota_aluno B on a.id_aluno = b.id_aluno  where a.id_aluno = 1000;

select A.*, B.nota,B.semestre  from graduacao.aluno A 
inner join 
(select * from graduacao.nota_aluno where semestre = 'primavera') B 
on a.id_aluno = b.id_aluno;

select A.*, B.nota,B.semestre, B.id_curso, C.titulo  from graduacao.aluno A 
inner join graduacao.nota_aluno B on a.id_aluno = b.id_aluno  
inner join graduacao.curso C on C.id_curso = B.id_curso
where a.id_aluno = 1000;


-------------------------left join------------------------------------
select 
a.*, 
b.nota,
b.semestre  
from graduacao.aluno a
left join graduacao.nota_aluno b 
on a.id_aluno = b.id_aluno ;

select 
a.*, 
b.nota,
b.semestre, 
b.id_curso, 
c.titulo  
from graduacao.curso c
left join 
graduacao.nota_aluno b 
on c.id_curso = b.id_curso
left join 
graduacao.aluno a  
on a.id_aluno = b.id_aluno 
where b.nota is not null;  

select 
a.*, 
B.nota,
B.semestre  
from graduacao.aluno a 
left join 
(select * from graduacao.nota_aluno 
where semestre = 'primavera') b 
on a.id_aluno = b.id_aluno 
where nota is null;


select 
c.id_curso,
b.nota,
b.semestre  
from graduacao.curso c 
left join  
graduacao.nota_aluno b 
on C.id_curso = B.id_curso
where b.nota is null;


-------------------------right join------------------------------------
select 
c.id_curso,
b.nota,
b.semestre  
from graduacao.nota_aluno b
right join  
graduacao.curso c   
on C.id_curso = B.id_curso where b.nota is null;


select 
b.id_aluno,
c.id_orientador,
b.nome,
a.id_curso,
d.titulo
from graduacao.orientador_aluno c 
right join  
graduacao.aluno b 
on c.id_aluno = b.id_aluno
left join  
graduacao.nota_aluno a   
on c.id_aluno = a.id_aluno
left join  
graduacao.curso d   
on a.id_curso = d.id_curso
where c.id_orientador is null;


update graduacao.orientador_aluno
set id_orientador = null
where id_aluno in('100','107','1110','15517','163','21401','288','3127','31624','41675','46035','50977','5381','62795','71025');

-------------------------full join------------------------------------
select 
*
from graduacao.orientador_aluno c 
full join  
graduacao.aluno b 
on c.id_aluno = b.id_aluno

-------------------------cross join------------------------------------
select 
b.* ,
c.*
from graduacao.nota_aluno b
cross join  
graduacao.curso c where b.id_aluno = '10663';

select 
b.* ,
c.*
from graduacao.nota_aluno b
cross join  
graduacao.curso c   
where b.id_curso = c.id_curso;

-------------------------utilizades join------------------------------------
update graduacao.instrutor 
set salario = salario *1.1
from graduacao.instrutor a 
inner join graduacao.professor b on b.id_professor = a.id_instrutor where a.salario < 40000;

select 
b.*,
a.salario,
a.nome
from graduacao.instrutor a 
inner join graduacao.professor b on b.id_professor = a.id_instrutor where a.salario < 50000;



delete graduacao.instrutor 
from graduacao.instrutor a 
inner join graduacao.professor b 
on b.id_professor = a.id_instrutor 
where a.salario < 40000;

select 
b.*,
a.salario,
a.nome
from graduacao.instrutor a 
inner join graduacao.professor b on b.id_professor = a.id_instrutor where a.salario < 50000;
