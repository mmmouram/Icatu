using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using backend.src.Controllers;
using backend.src.Models.DTOs;
using backend.src.Services;

namespace backend.test.Controllers
{
    [TestFixture]
    public class SinistroControllerTests
    {
        private Mock<ISinistroService> _sinistroServiceMock;
        private SinistroController _controller;

        [SetUp]
        public void Setup()
        {
            _sinistroServiceMock = new Mock<ISinistroService>();
            _controller = new SinistroController(_sinistroServiceMock.Object);
        }

        [Test]
        public async Task RegistrarSinistro_RetornaCreatedResult_ComDadosCorretos()
        {
            // Arrange
            var request = new SinistroRequest { Descricao = "Sinistro Teste" };
            var response = new SinistroResponse { Id = 1, DataRegistro = System.DateTime.UtcNow, Status = "Registrado" };
            _sinistroServiceMock
                .Setup(s => s.RegistrarSinistroAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.RegistrarSinistro(request);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var createdResult = (CreatedAtActionResult)result.Result;
            Assert.AreEqual("AcompanharSinistro", createdResult.ActionName);
            Assert.AreEqual(response, createdResult.Value);
        }

        [Test]
        public async Task EnviarDocumento_ComDocumentoNulo_RetornaBadRequest()
        {
            // Arrange
            int sinistroId = 1;

            // Act
            var result = await _controller.EnviarDocumento(sinistroId, null);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result.Result);
            var badRequest = (BadRequestObjectResult)result.Result;
            Assert.AreEqual("Documento inválido.", badRequest.Value);
        }

        [Test]
        public async Task EnviarDocumento_ComDocumentoValido_RetornaOkResult()
        {
            // Arrange
            int sinistroId = 1;
            var documentoMock = new Mock<IFormFile>();
            documentoMock.Setup(f => f.Length).Returns(100);
            documentoMock.Setup(f => f.FileName).Returns("teste.pdf");
            
            var documentoResponse = new backend.src.Models.DTOs.DocumentoResponse
            {
                DocumentoId = 10,
                NomeArquivo = "teste.pdf",
                Mensagem = "Documento enviado com sucesso."
            };

            _sinistroServiceMock
                .Setup(s => s.EnviarDocumentoAsync(sinistroId, It.IsAny<IFormFile>()))
                .ReturnsAsync(documentoResponse);

            // Act
            var result = await _controller.EnviarDocumento(sinistroId, documentoMock.Object);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = (OkObjectResult)result.Result;
            Assert.AreEqual(documentoResponse, okResult.Value);
        }

        [Test]
        public async Task AcompanharSinistro_SinistroNaoEncontrado_RetornaNotFound()
        {
            // Arrange
            int sinistroId = 999;
            _sinistroServiceMock
                .Setup(s => s.AcompanharSinistroAsync(sinistroId))
                .ReturnsAsync((SinistroResponse)null);

            // Act
            var result = await _controller.AcompanharSinistro(sinistroId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result.Result);
            var notFoundResult = (NotFoundObjectResult)result.Result;
            Assert.AreEqual("Sinistro não encontrado.", notFoundResult.Value);
        }

        [Test]
        public async Task AcompanharSinistro_SinistroEncontrado_RetornaOkResult()
        {
            // Arrange
            int sinistroId = 1;
            var sinistroResponse = new SinistroResponse
            {
                Id = sinistroId,
                DataRegistro = System.DateTime.UtcNow,
                Status = "Registrado"
            };
            _sinistroServiceMock
                .Setup(s => s.AcompanharSinistroAsync(sinistroId))
                .ReturnsAsync(sinistroResponse);

            // Act
            var result = await _controller.AcompanharSinistro(sinistroId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = (OkObjectResult)result.Result;
            Assert.AreEqual(sinistroResponse, okResult.Value);
        }
    }
}
