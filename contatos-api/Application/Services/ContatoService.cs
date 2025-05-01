using Domain.Exceptions.ContatoExceptions;
using Domain.Interfaces;
using Domain.Interfaces.ContatoInterfaces;

namespace Application.Services
{
    public class ContatoService : IContatoService
    {
        private readonly IContatoRepository contatoRepository;
        private readonly IEventPublisher eventPublisher;

        public ContatoService(IContatoRepository contatoRepository, IEventPublisher eventPublisher)
        {
            this.contatoRepository = contatoRepository;
            this.eventPublisher = eventPublisher;
        }

        public async Task<Domain.Entities.Contato> AtualizarContatoAsync(Domain.Entities.Contato contato)
        {
            contato.Validate();
            ValidaSeContatoExiste(contato.ID);

            var updatedContato = contatoRepository.Save(contato);

            await eventPublisher.PublishAsync("ContatoAtualizadoExchange", "", updatedContato, default);

            return updatedContato;
        }

        private void ValidaSeContatoExiste(int id)
        {
            BuscarPorId(id);
        }

        public Domain.Entities.Contato BuscarPorId(int id)
        {
            var dbContato = contatoRepository.FindById(id);
            if (dbContato == null) throw new ContatoNaoEncontradoException();
            return dbContato;
        }

        public async Task<Domain.Entities.Contato> CadastrarContatoAsync(Domain.Entities.Contato contato)
        {
            contato.Validate();
            var dbContato = contatoRepository.FindByCodigoAreaAndTelefone(contato.CodigoArea, contato.Telefone);
            if (dbContato != null)
                throw new ContatoJaCadastradoException();

            var savedContato = contatoRepository.Save(contato);

            await eventPublisher.PublishAsync("ContatoAtualizadoExchange", "", savedContato, default);

            return savedContato;
        }

        public async Task ExcluirContatoAsync(int id)
        {
            var contato = contatoRepository.FindById(id);
            if (contato != null)
            {
                contato.Remove();
                contatoRepository.Save(contato);

                await eventPublisher.PublishAsync("ContatoAtualizadoExchange", "", contato, default);
            }
        }

        public List<Domain.Entities.Contato> ListarContatos(int? codigoArea = null)
        {
            if (codigoArea == null) return contatoRepository.FindAll().ToList();

            return contatoRepository.FindByCodigoArea(codigoArea.Value);
        }
    }
}
