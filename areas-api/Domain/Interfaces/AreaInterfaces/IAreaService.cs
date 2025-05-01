using Domain.Entities;

namespace Domain.Interfaces.AreaInterfaces
{
    public interface IAreaService
    {
        Entities.Area BuscarPorCodigoArea(int codigoArea);
        Task<List<Entities.Area>> CadastrarAreasAsync(List<Entities.Area> areas, CancellationToken cancellationToken = default);
    }
}
