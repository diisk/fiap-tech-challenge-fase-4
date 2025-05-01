using Domain.Entities;

namespace Domain.Interfaces.AreaInterfaces
{
    public interface IAreaService
    {
        Entities.Area BuscarPorCodigoArea(int codigoArea);
    }
}
