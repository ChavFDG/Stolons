
$.extend(jQuery.validator.messages, {
    min: jQuery.validator.format("Doit être supérieur ou égal à {0}."),
    max: jQuery.validator.format("Doit être inférieur ou égal à {0}."),
    required: "Cette valeur est requise",
    email: "Doit être une adresse email valide",
    url: "Doit être une URL valide",
    date: "Doit être une date valide",
    number: "Doit être un nombre valide",
});

// Sauvegarde les méthodes de base
var originalMethods = {
    min: $.validator.methods.min,
    max: $.validator.methods.max,
    range: $.validator.methods.range
};

// Analyse un nombre
var parseFrenchNum = function (str) {
    str = str.replace(",", ".").replace(" ", "");
    return parseFloat(str);
};

// Analyse une date
var parseFrenchDate = function (str) {
    return new Date(str);
};

// Traitement des nombres
$.validator.methods.number = function (value, element) {
    var val = parseFrenchNum(value);
    return this.optional(element) || ($.isNumeric(val));
};

// Traitement des dates
$.validator.methods.date = function (value, element) {
    var val = parseFrenchDate(value);
    return this.optional(element) || (val instanceof Date);
};

// Traitement des règles sur les nombres
$.validator.methods.min = function (value, element, param) {
    var val = parseFrenchNum(value);
    return originalMethods.min.call(this, val, element, param);
};

$.validator.methods.max = function (value, element, param) {
    var val = parseFrenchNum(value);
    return originalMethods.max.call(this, val, element, param);
};

$.validator.methods.range = function (value, element, param) {
    var val = parseFrenchNum(value);
    return originalMethods.range.call(this, val, element, param);
};
