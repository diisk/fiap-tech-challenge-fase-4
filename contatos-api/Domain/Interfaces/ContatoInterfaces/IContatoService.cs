using Domain.Entities;

namespace Domain.Interfaces.ContatoInterfaces
{
    public interface IContatoService
    {
        List<Entities.Contato> ListarContatos(int? codigoArea = null);
        Task<Entities.Contato> CadastrarContatoAsync(Entities.Contato contato);
        Entities.Contato BuscarPorId(int id);
        Task<Entities.Contato> AtualizarContatoAsync(Entities.Contato contato);
        Task ExcluirContatoAsync(int id);
    }
}
