﻿/*!
 * jQuery Translations plugin
 *
 * see https://github.com/wsidl/jquery-translate
 *
 * Licensed under the MIT license.
 *
 * Version: 1.3 (19 Aug 2015)
 */
(function ($) { p = { o: $.fn.text, code_index: {}, j: {}, k: undefined, d: [], default_language: "en", add_codes: function (a) { p.j = $.extend({}, p.j, a); return p; }, set_language: function (l) { if (!(l == undefined)) { if (l.length > 2 && l.indexOf("-") >= 0) { l = l.split("-")[0]; } } else { l = (navigator.language || navigator.userLanguage || navigator.systemLanguage).split("-")[0]; } if (!(l in p.code_index)) { console.log("Language not supported, add index for", l); l = p.default_language; } if (p.k == undefined) { p.k = l; } var ol = p.get_language(); if (p.k == p.code_index[l]) { return this; } p.k = p.code_index[l]; $.each(p.j, function (a, b) { if (a.substr(0, 1) == "!") { var c = $(a.substr(1)); if (!(c.is("button") || c.is("input"))) { c.html(p.get_text(a)); } else { c.val(p.get_text(a)); } } }); $.each(p.d, function (a, b) { try { b(ol, l); } catch (e) { console.log(e); } }); return this; }, get_language: function () { var d = p.default_language; $.each(p.code_index, function (k, v) { if (v == p.k) { return d = k; } }); return d; }, get_text: function (k, o) { if (k == "") { return this; } var r = ""; try { var t = p.j[k][p.k]; var e = $.extend({}, p.j, o); if (e) { $.each(e, function (a, b) { var c = new RegExp("{{" + a + "}}", "g"); var d = (typeof b == "string" ? b : b[p.k]); t = t.replace(c, d); }); } if (t.match(/{{.*}}/)) { $.error("Missing option for key '" + k + "'"); } r = t; } catch (e) { r = k; } if (this == p) { return r; } else { return $.proxy(p.o, this, r)(); } }, change: function (c) { if (typeof c == "function") { p.d.push(c); } return this; } }; $.translate = p; $.fn.text = p.get_text; })(jQuery);
