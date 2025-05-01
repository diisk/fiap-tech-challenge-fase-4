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

        public async Task<List<Domain.Entities.Area>> CadastrarAreasAsync(List<Domain.Entities.Area> areas, CancellationToken cancellationToken = default)
        {
            if (areas.Count == 0)
            {
                throw new ConteudoDiferenteException();
            }

            List<int> codigos = [];
            foreach (var area in areas)
            {
                area.Validate();

                if (codigos.Contains(area.Codigo))
                    throw new CodigoAreaDuplicadoException();

                codigos.Add(area.Codigo);
            }

            if (areaRepository.FindByCodigo(codigos).Count > 0)
                throw new CodigoAreaCadastradoException(codigos);

            var savedAreas = areaRepository.SaveAll(areas);

            foreach (var area in savedAreas)
            {
                await eventPublisher.PublishAsync("AreaAtualizadaExchange", "", area, cancellationToken);
            }

            return savedAreas;
        }
    }
}
