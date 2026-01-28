using System;

namespace TP2_P1.Models.EntityFramework
{
    // Le mot-clé 'partial' permet de fusionner cette classe avec celle générée par EF
    public partial class Serie
    {
        // Constructeur par défaut (déjà présent dans la classe générée, mais peut être ajouté ici si besoin)

        // Implémentation de Equals pour faciliter les tests unitaires
        public override bool Equals(object? obj)
        {
            return obj is Serie serie &&
                   Serieid == serie.Serieid &&
                   Titre == serie.Titre &&
                   Resume == serie.Resume &&
                   Nbsaisons == serie.Nbsaisons &&
                   Nbepisodes == serie.Nbepisodes &&
                   Anneecreation == serie.Anneecreation &&
                   Network == serie.Network;
        }

        // Toujours redéfinir GetHashCode quand on redéfinit Equals
        public override int GetHashCode()
        {
            return HashCode.Combine(Serieid, Titre, Resume, Nbsaisons, Nbepisodes, Anneecreation, Network);
        }
    }
}