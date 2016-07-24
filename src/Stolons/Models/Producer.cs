using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public class Producer : User
    {
        [Display(Name = "Raison sociale")]
        public string CompanyName { get; set; }
        [Display(Name = "Superficie en m²")]
        public int Area { get; set; }

        private IList<string> _ExploitationPictures;
        [Display(Name = "Galerie d'exploitations")]
        [NotMapped]
        public IList<string> ExploitationPictures
        {
            get
            {

                return _ExploitationPictures;

            }
            set
            {
                _ExploitationPictures = value;
            }
        }
        public string ExploitationPicuresSerialized
        {
            get
            {
                if (_ExploitationPictures == null)
                {
                    return null;
                }
                else
                {
                    return String.Join(";", _ExploitationPictures);
                }
            }
            set
            {
                _ExploitationPictures = Tools.SerializeStringToList(value);
            }
        }

        [Display(Name = "Production")]
        public string Production { get; set; }
        [Display(Name = "Texte libre")]
        public string OpenText { get; set; }
        [Display(Name = "Année d'installation")]
        public int StartDate { get; set; }
        [Display(Name = "Lien vers le site web")]
        [Url]
        public string WebSiteLink { get; set; }
        [Display(Name = "Factures")]
        public List<ProducerBill> Bills { get; set; }
	    [Display(Name = "Latitude")]
        public double Latitude { get; set; }
	    [Display(Name = "Longitude")]
        public double Longitude { get; set; }
    }
}
