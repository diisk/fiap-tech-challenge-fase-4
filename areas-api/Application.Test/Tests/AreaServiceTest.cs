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

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void CadastrarAreasAsync_QuandoCodigoInvalido_DeveLancarExcecao(int codigo)
        {
            //GIVEN
            var mockRepository = new Mock<IAreaRepository>();
            var eventPublisher = new Mock<IEventPublisher>();

            Area area = fixture.AreaValida;
            area.Codigo = codigo;


            var areaService = new AreaService(mockRepository.Object, eventPublisher.Object);

            //WHEN & THEN
            Assert.ThrowsAsync<ValidacaoException>(async () => await areaService.CadastrarAreasAsync([area]));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("S")]
        [InlineData("SSS")]
        public void CadastrarAreasAsync_QuandoSiglaInvalida_DeveLancarExcecao(string siglaEstado)
        {
            //GIVEN
            var mockRepository = new Mock<IAreaRepository>();
            var eventPublisher = new Mock<IEventPublisher>();

            Area area = fixture.AreaValida;
            area.SiglaEstado = siglaEstado;

            var areaService = new AreaService(mockRepository.Object, eventPublisher.Object);

            //WHEN & THEN
            Assert.ThrowsAsync<ValidacaoException>(async () => await areaService.CadastrarAreasAsync([area]));
        }

        [Fact]
        public void CadastrarAreasAsync_QuandoCodigoAreaJaCadastrado_DeveLancarExcecao()
        {
            //GIVEN
            var mockRepository = new Mock<IAreaRepository>();
            var eventPublisher = new Mock<IEventPublisher>();

            Area area = fixture.AreaValida;
            var codigos = new List<int>();
            codigos.Add(area.Codigo);

            mockRepository.Setup(repo => repo.FindByCodigo(codigos)).Returns([area]);

            var areaService = new AreaService(mockRepository.Object, eventPublisher.Object);

            //WHEN & THEN
            Assert.ThrowsAsync<CodigoAreaCadastradoException>(async () => await areaService.CadastrarAreasAsync([area]));
        }

        [Fact]
        public void CadastrarAreasAsync_QuandoDadosCodigoDuplicado_DeveRetornaNovaListaEntidade()
        {
            //GIVEN
            var mockRepository = new Mock<IAreaRepository>();
            var eventPublisher = new Mock<IEventPublisher>();

            Area area1 = fixture.AreaValida;
            Area area2 = fixture.AreaValida;

            List<int> codigos = [area1.Codigo, area2.Codigo];
            List<Area> areas = [area1, area2];

            mockRepository.Setup(repo => repo.FindByCodigo(codigos)).Returns([]);

            var areaService = new AreaService(mockRepository.Object, eventPublisher.Object);

            //WHEN & THEN
            Assert.ThrowsAsync<CodigoAreaDuplicadoException>(async () => await areaService.CadastrarAreasAsync(areas));
        }

        [Fact]
        public void CadastrarAreasAsync_QuandoDadosVazios_DeveRetornaNovaListaEntidade()
        {
            //GIVEN
            var mockRepository = new Mock<IAreaRepository>();
            var eventPublisher = new Mock<IEventPublisher>();

            var areaService = new AreaService(mockRepository.Object, eventPublisher.Object);

            //WHEN & THEN
            Assert.ThrowsAsync<ConteudoDiferenteException>(async () => await areaService.CadastrarAreasAsync([]));
        }

        [Fact]
        public async void CadastrarAreasAsync_QuandoDadosValidos_DeveRetornaNovaListaEntidade()
        {
            //GIVEN
            var mockRepository = new Mock<IAreaRepository>();
            var eventPublisher = new Mock<IEventPublisher>();

            Area area1 = fixture.AreaValida;
            area1.Codigo = 11;
            Area area2 = fixture.AreaValida;
            area2.Codigo = 31;
            Area areaRetorno1 = fixture.AreaValida;
            areaRetorno1.ID = 1;
            Area areaRetorno2 = fixture.AreaValida;
            areaRetorno2.ID = 2;

            List<int> codigos = [area1.Codigo, area2.Codigo];
            List<Area> areas = [area1, area2];
            List<Area> areasRetorno = [areaRetorno1, areaRetorno2];

            mockRepository.Setup(repo => repo.SaveAll(areas)).Returns(areasRetorno);
            mockRepository.Setup(repo => repo.FindByCodigo(codigos)).Returns([]);

            var areaService = new AreaService(mockRepository.Object, eventPublisher.Object);

            //WHEN
            var retorno = await areaService.CadastrarAreasAsync([area1, area2]);

            //THEN
            Assert.Equal(retorno, areasRetorno);
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