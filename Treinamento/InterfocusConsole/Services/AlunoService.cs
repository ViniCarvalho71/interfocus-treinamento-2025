using System.ComponentModel.DataAnnotations;
using InterfocusConsole.Models;
using InterfocusConsole.Repository;

namespace InterfocusConsole.Services
{
    public class AlunoService
    {
        private readonly IRepository repository;

        public AlunoService(IRepository repository)
        {
            this.repository = repository;
        }
        
        public bool Cadastrar(Aluno aluno, out List<MensagemErro> mensagens)
        {
            var valido = Validar(aluno, out mensagens);
            if (valido)
            {
                repository.Incluir(aluno);
                return true;
            }
            return false;
        }

        public bool Validar(Aluno aluno, out List<MensagemErro> mensagens)
        {
            var validationContext = new ValidationContext(aluno);
            var erros = new List<ValidationResult>();
            var validation = Validator.TryValidateObject(aluno,
                validationContext,
                erros,
                true);
            var isEmailAlreadyRegistered = repository.Consultar<Aluno>().
                Count(a => a.Email == aluno.Email);
            mensagens = new List<MensagemErro>();
            if (isEmailAlreadyRegistered > 0)
            {
                var mensagem = new MensagemErro(
                    "Email",
                    "Email já cadastrado"
                );
                mensagens.Add(mensagem);
                validation = false;
            }
            
            foreach (var erro in erros)
            {
                var mensagem = new MensagemErro(
                    erro.MemberNames.First(),
                    erro.ErrorMessage);

                mensagens.Add(mensagem);
                Console.WriteLine("{0}: {1}",
                    erro.MemberNames.First(),
                    erro.ErrorMessage);
            }

            if (aluno.Idade < 16)
            {
                mensagens.Add(new MensagemErro("dataNascimento", "Aluno deve ser maior de 16 anos"));
                validation = false;
            }

            //throw new Exception("dados invalidos!!!!");
            return validation;
        }

        public List<Aluno> Consultar(int page, int tamanhoPagina)
        {
            return repository.Consultar<Aluno>()
                .OrderBy(item => item.Nome)
                .Skip((page - 1) * tamanhoPagina)
                .Take(tamanhoPagina)
                .ToList();

        }

        public List<Aluno> Consultar(string pesquisa)
        {
            bool FiltraLista(Aluno item)
            {
                return item.Nome.Contains(pesquisa);
            }
            // lambda expression
            var resultado2 = repository
                .Consultar<Aluno>()
                .Where(item => item.Nome.Contains(pesquisa))
                .OrderBy(item => item.Nome)
                .Take(10)
                .ToList();
            return resultado2;
        }

        public Aluno ConsultarPorCodigo(long codigo)
        {
            return repository.ConsultarPorId<Aluno>(codigo);
        }

        public Aluno Editar(Aluno aluno)
        {
            var existente = ConsultarPorCodigo(aluno.Id);

            if (existente == null)
            {
                return null;
            }
            existente.Nome = aluno.Nome;
            repository.Salvar(existente);
            return existente;
        }

        public Aluno Deletar(long codigo)
        {
            var existente = ConsultarPorCodigo(codigo);
            repository.Excluir(existente);
            return existente;
        }

        public List<Aluno> Aniversariantes()
        {
            var hoje = DateTime.Today;
            var aniversariantes = repository.Consultar<Aluno>().
                Where(a => a.DataNascimento.Value.Day.Equals(hoje.Day) &&
                           a.DataNascimento.Value.Month.Equals(hoje.Month)).ToList();
            
            return aniversariantes;
        }
    }
}
