using Application.Exceptions;
using Domain.Entities;
using Domain.Exceptions.AreaExceptions;
using Domain.Interfaces;
using Domain.Interfaces.AreaInterfaces;

namespace Application.Services
{
    public class AreaService : IAreaService
    {
        private readonly IAreaRepository areaRepository;
        private readonly IEventPublisher eventPublisher;

        public AreaService(IAreaRepository areaRepository, IEventPublisher eventPublisher)
        {
            this.areaRepository = areaRepository;
            this.eventPublisher = eventPublisher;
        }

        public Domain.Entities.Area BuscarPorCodigoArea(int codigoArea)
        {
            var area = areaRepository.FindByCodigo([codigoArea]);
            if (area.Count == 0) throw new CodigoAreaNaoCadastradoException();

            return area.First();
        }

    }
}
