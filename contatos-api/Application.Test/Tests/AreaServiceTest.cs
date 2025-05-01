using Application.Exceptions;
using Application.Services;
using Application.Test.Fixtures;
using Domain.Entities;
using Domain.Exceptions.AreaExceptions;
using Domain.Interfaces;
using Domain.Interfaces.AreaInterfaces;
using Moq;

namespace Application.Test.Tests
{
    [Trait("Category","Unit")]
    public class AreaServiceTest : IClassFixture<AreaFixture>
    {
        
        private readonly AreaFixture fixture;

        public AreaServiceTest(AreaFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void BuscarPorCodigoArea_QuandoAreaNaoCadastrada_DeveLancarExcecao()
        {
            //GIVEN
            var mockRepository = new Mock<IAreaRepository>();
            var eventPublisher = new Mock<IEventPublisher>();
            var codigoArea = 11;
            List<int> codigos = [codigoArea];

            var areaService = new AreaService(mockRepository.Object, eventPublisher.Object);
            mockRepository.Setup(repo => repo.FindByCodigo(codigos)).Returns([]);

            //WHEN & THEN
            Assert.Throws<CodigoAreaNaoCadastradoException>(() => areaService.BuscarPorCodigoArea(codigoArea));
        }

        [Fact]
        public void BuscarPorCodigoArea_QuandoAreaCadastrada_DeveRetornaArea()
        {
            //GIVEN
            var mockRepository = new Mock<IAreaRepository>();
            var eventPublisher = new Mock<IEventPublisher>();
            var area = fixture.AreaValida;
            var codigoArea = 11;
            area.Codigo = codigoArea;
            var codigos = new List<int>();
            codigos.Add(codigoArea);

            mockRepository.Setup(repo => repo.FindByCodigo(codigos)).Returns([area]);

            var areaService = new AreaService(mockRepository.Object, eventPublisher.Object);

            //WHEN
            var retorno = areaService.BuscarPorCodigoArea(codigoArea);

            //THEN
            Assert.Equal(retorno.Codigo,codigoArea);
        }

    }
}