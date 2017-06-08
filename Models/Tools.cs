using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stolons.Models
{
    public static class Tools
    {
        public static IList<string> SerializeStringToList( string stringToSerialize)
        {
            if (String.IsNullOrWhiteSpace(stringToSerialize))
            {
                return new List<string>();
            }
            else
            {
                return stringToSerialize.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        public static string SerializeListToString(IList<string> listToSerialize)
        {
            if (listToSerialize == null)
                return null;
            return String.Join(";", listToSerialize);
        }

        public static void SetLabels(this Product product, string[] labels)
        {
            if (labels == null)
                return;
            foreach (string label in labels)
            {
                product.Labels.Add((Product.Label)Enum.Parse(typeof(Product.Label), label));
            }
        }
    }
}
