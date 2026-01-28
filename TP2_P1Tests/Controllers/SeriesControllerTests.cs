using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TP2_P1.Controllers;
using TP2_P1.Models.EntityFramework;

namespace TP2_P1.Controllers.Tests
{
    [TestClass]
    public class SeriesControllerTests
    {
        private const string ConnectionString =
            "Server=localhost;port=5432;Database=SeriesDB;uid=postgres;password=postgres;";

        private SeriesDbContext _context = null!;
        private SeriesController _controller = null!;

        [TestInitialize]
        public void Init()
        {
            var options = new DbContextOptionsBuilder<SeriesDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

            _context = new SeriesDbContext(options);
            _controller = new SeriesController(_context);
        }

        [TestMethod]
        public void GetSeries_OK_LimitTo3_FirstOnesMatch()
        {
            // Act
            var actionResult = _controller.GetSeries().Result;
            var series = actionResult.Value!.OrderBy(s => s.Serieid).ToList();

            var limited = series.Where(s => s.Serieid <= 3).OrderBy(s => s.Serieid).ToList();

            // Assert
            Assert.AreEqual(3, limited.Count);

            var expected1 = new Serie
            {
                Serieid = 1,
                Titre = "Scrubs",
                Resume = "J.D. est un jeune médecin qui débute sa carrière dans l'hôpital du Sacré-Coeur. Il vit avec son meilleur ami Turk, qui lui est chirurgien dans le même hôpital. Très vite, Turk tombe amoureux d'une infirmière Carla. Elliot entre dans la bande. C'est une étudiante en médecine quelque peu surprenante. Le service de médecine est dirigé par l'excentrique Docteur Cox alors que l'hôpital est géré par le diabolique Docteur Kelso. A cela viennent s'ajouter plein de personnages hors du commun : Todd le chirurgien obsédé, Ted l'avocat dépressif, le concierge qui trouve toujours un moyen d'embêter JD... Une belle galerie de personnage !",
                Nbsaisons = 9,
                Nbepisodes = 184,
                Anneecreation = 2001,
                Network = "ABC (US)"
            };

            var expected2 = new Serie
            {
                Serieid = 2,
                Titre = "James May's 20th Century",
                Resume = "The world in 1999 would have been unrecognisable to anyone from 1900. James May takes a look at some of the greatest developments of the 20th century, and reveals how they shaped the times we live in now.",
                Nbsaisons = 1,
                Nbepisodes = 6,
                Anneecreation = 2007,
                Network = "BBC Two"
            };

            var expected3 = new Serie
            {
                Serieid = 3,
                Titre = "True Blood",
                Resume = "Ayant trouvé un substitut pour se nourrir sans tuer (du sang synthétique), les vampires vivent désormais parmi les humains. Sookie, une serveuse capable de lire dans les esprits, tombe sous le charme de Bill, un mystérieux vampire. Une rencontre qui bouleverse la vie de la jeune femme...",
                Nbsaisons = 7,
                Nbepisodes = 81,
                Anneecreation = 2008,
                Network = "HBO"
            };

            AssertSerieEquals(expected1, limited[0]);
            AssertSerieEquals(expected2, limited[1]);
            AssertSerieEquals(expected3, limited[2]);
        }

        [TestMethod]
        public void GetSerie_OK_WhenExists()
        {
            // Arrange
            int existingId = 1;

            // Act
            var result = _controller.GetSerie(existingId).Result;

            // Assert
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(existingId, result.Value!.Serieid);
        }

        [TestMethod]
        public void GetSerie_NotFound_WhenMissing()
        {
            // Arrange
            int missingId = 999999;

            // Act
            var result = _controller.GetSerie(missingId).Result;

            // Assert
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void DeleteSerie_OK_WhenExists()
        {
            // Arrange: on crée une série temporaire pour ne pas casser les données du script
            var temp = new Serie
            {
                Titre = "TMP_DELETE_" + Guid.NewGuid(),
                Resume = "tmp",
                Nbsaisons = 1,
                Nbepisodes = 1,
                Anneecreation = 2024,
                Network = "TMP"
            };

            _context.Series.Add(temp);
            _context.SaveChanges();
            int id = temp.Serieid;

            // Act
            var deleteResult = _controller.DeleteSerie(id).Result;

            // Assert
            Assert.IsInstanceOfType(deleteResult, typeof(NoContentResult));

            var stillThere = _context.Series.Find(id);
            Assert.IsNull(stillThere);
        }

        [TestMethod]
        public void DeleteSerie_NotFound_WhenMissing()
        {
            // Arrange
            int missingId = 999999;

            // Act
            var result = _controller.DeleteSerie(missingId).Result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void PostSerie_ThrowsAggregateException_WhenTitreMissing()
        {
            // Arrange: Titre est NOT NULL en base => SaveChangesAsync va échouer
            var invalid = new Serie
            {
                // Titre manquant volontairement
                Resume = "no titre",
                Nbsaisons = 1,
                Nbepisodes = 1,
                Anneecreation = 2024,
                Network = "X"
            };

            // Act + Assert
            Assert.ThrowsException<AggregateException>(() =>
            {
                _ = _controller.PostSerie(invalid).Result;
            });
        }

        [TestMethod]
        public void PutSerie_OK_UpdatesEntity()
        {
            // Arrange: créer une série temporaire
            var temp = new Serie
            {
                Titre = "TMP_PUT_" + Guid.NewGuid(),
                Resume = "before",
                Nbsaisons = 1,
                Nbepisodes = 1,
                Anneecreation = 2024,
                Network = "TMP"
            };

            _context.Series.Add(temp);
            _context.SaveChanges();
            int id = temp.Serieid;

            // Modifier
            temp.Resume = "after";

            // Act
            var result = _controller.PutSerie(id, temp).Result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(NoContentResult));

            var reloaded = _context.Series.Find(id);
            Assert.IsNotNull(reloaded);
            Assert.AreEqual("after", reloaded!.Resume);

            // Cleanup
            _context.Series.Remove(reloaded);
            _context.SaveChanges();
        }

        [TestMethod]
        public void PutSerie_BadRequest_WhenRouteIdDiffersFromBodyId()
        {
            // Arrange
            var serie = new Serie
            {
                Serieid = 123,
                Titre = "X",
                Resume = "Y"
            };

            // Act
            var result = _controller.PutSerie(999, serie).Result;

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        private static void AssertSerieEquals(Serie expected, Serie actual)
        {
            Assert.AreEqual(expected.Serieid, actual.Serieid);
            Assert.AreEqual(expected.Titre, actual.Titre);
            Assert.AreEqual(expected.Resume, actual.Resume);
            Assert.AreEqual(expected.Nbsaisons, actual.Nbsaisons);
            Assert.AreEqual(expected.Nbepisodes, actual.Nbepisodes);
            Assert.AreEqual(expected.Anneecreation, actual.Anneecreation);
            Assert.AreEqual(expected.Network, actual.Network);
        }
    }
}