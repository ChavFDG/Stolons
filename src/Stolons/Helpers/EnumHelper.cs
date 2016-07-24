﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Stolons.Models;

namespace Stolons.Helpers
{
    public static class EnumHelper<T>
    {
        public static IList<T> GetValues(Enum value)
        {
            var enumValues = new List<T>();

            foreach (FieldInfo fi in value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                enumValues.Add((T)Enum.Parse(value.GetType(), fi.Name, false));
            }
            return enumValues;
        }

        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static IList<string> GetNames(Enum value)
        {
            return value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public).Select(fi => fi.Name).ToList();
        }

        public static IList<string> GetDisplayValues(Enum value)
        {
            return GetNames(value).Select(obj => GetDisplayValue(Parse(obj))).ToList();
        }

        public static string GetDisplayValue(T value)
        {
            if (typeof(T) == typeof(DayOfWeek))
            {
                DayOfWeek dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek),value.ToString()) ;
                return dayOfWeek.ToFrench();
            }
            else
            {
                var fieldInfo = value.GetType().GetField(value.ToString());

                var descriptionAttributes = fieldInfo.GetCustomAttributes(
                    typeof(DisplayAttribute), false) as DisplayAttribute[];

                if (descriptionAttributes == null) return string.Empty;
                return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();
            }
        }
    }

    public static class EnumHelper
    {
        public static string ToFrench(this DayOfWeek dayOfWeekValue)
        {
	    var culture = new System.Globalization.CultureInfo("fr-FR");
	    return culture.DateTimeFormat.GetDayName(dayOfWeekValue).ToUpper();
        }
    }

    public static class ProductHelper
    {
	public static string GetStockUnit(this Product product)
	{
	    if (product.Type == Product.SellType.Piece)
	    {
		return " Pièces";
	    } else
	    {
		if (product.ProductUnit == Product.Unit.Kg)
		{
		    return " Kg";
		} else {
		    return " L";
		}
	    }
	}
    }
}
