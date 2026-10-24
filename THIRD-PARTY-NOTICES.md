# Third-party notices

Taskist is licensed under the [MIT License](LICENSE). It includes or depends on the third-party software listed
below, each under its own licence. The full licence text of each project is available at its homepage and, for
bundled files, in the header of the file itself.

## Bundled front-end libraries

These files are vendored in this repository under `src/Presentation/Taskist.Web/wwwroot/lib`,
`wwwroot/theme` and `wwwroot/icon-fonts`, and are served to the browser.

| Library | Version | Licence |
| ------- | ------- | ------- |
| [Bootstrap](https://getbootstrap.com) | 5.3.3 | MIT |
| [CoreUI](https://coreui.io) | 5.1.2 | MIT |
| [CoreUI Chart.js](https://github.com/coreui/coreui-chartjs) / [Chart.js](https://www.chartjs.org) | 4.x | MIT |
| [Font Awesome Free](https://fontawesome.com) | 6.4.0 | Icons: CC BY 4.0 · Fonts: SIL OFL 1.1 · Code: MIT |
| [jQuery](https://jquery.com) | 3.7.1 | MIT |
| [jQuery UI](https://jqueryui.com) | 1.13.0 | MIT |
| [jQuery Validation](https://jqueryvalidation.org) | bundled | MIT |
| [jQuery Unobtrusive Validation / Ajax](https://github.com/aspnet/jquery-validation-unobtrusive) | bundled | Apache-2.0 |
| [jQuery BlockUI](https://github.com/malsup/blockui) | bundled | MIT (dual MIT/GPL; used under MIT) |
| [jQuery.print](https://github.com/DoersGuild/jQuery.print) | 1.6.2 | CC BY 3.0 |
| [jquery-confirm](https://craftpip.github.io/jquery-confirm/) | 3.3.4 | MIT |
| [jQuery.skedTape](https://github.com/lexkrstn/jquery-sked-tape) | 2.4.6 | MIT |
| [DataTables](https://datatables.net) + RowGroup | 1.13.11 / 1.5.1 | MIT |
| [Select2](https://select2.org) | bundled | MIT |
| [SweetAlert2](https://sweetalert2.github.io) | bundled | MIT |
| [toastr](https://github.com/CodeSeven/toastr) | bundled | MIT |
| [Ladda](https://github.com/hakimel/Ladda) | bundled | MIT |
| [Node Waves](https://github.com/fians/Waves) | 0.7.6 | MIT |
| [SimpleBar](https://github.com/Grsmto/simplebar) | bundled | MIT |
| [Popper](https://popper.js.org) | 2.11.8 | MIT |
| [bootstrap-datepicker](https://github.com/uxsolutions/bootstrap-datepicker) | 1.9.0 | Apache-2.0 |
| [Ion.RangeSlider](https://github.com/IonDen/ion.rangeSlider) | 2.3.1 | MIT |
| [Moment.js](https://momentjs.com) | bundled | MIT |
| [FullCalendar](https://fullcalendar.io) (standard edition) | 6.1.15 | MIT |
| [jsTree](https://www.jstree.com) | 3.3.17 | MIT |
| [Cropper.js](https://github.com/fengyuanchen/cropperjs) | 1.6.2 | MIT |
| [Dropzone](https://www.dropzone.dev) | bundled | MIT |
| [PivotTable.js](https://pivottable.js.org) | bundled | MIT |
| [plotly.js](https://plotly.com/javascript/) | 1.58.5 | MIT |
| [jsPDF](https://github.com/parallax/jsPDF) + AutoTable | bundled / 3.5.25 | MIT |
| [SheetJS Community Edition](https://sheetjs.com) | 0.18.5 | Apache-2.0 |
| [ASP.NET Core SignalR client](https://github.com/dotnet/aspnetcore) | bundled | MIT |
| [TinyMCE](https://www.tiny.cloud) | 7.5.1 | **GPL-2.0-or-later** — see note below |

> [!NOTE]
> **TinyMCE 7 is licensed under the GPL**, not MIT. Taskist uses it, unmodified, as the rich-text editor. If you
> distribute Taskist, or a product built on it, the GPL applies to the editor files in `wwwroot/lib/tinymce`.
> Its licence is in [`wwwroot/lib/tinymce/license.md`](src/Presentation/Taskist.Web/wwwroot/lib/tinymce/license.md).

## NuGet packages

Restored at build time; not stored in this repository.

| Package | Licence |
| ------- | ------- |
| Microsoft.AspNetCore.*, Microsoft.EntityFrameworkCore.*, System.Security.Cryptography.Pkcs | MIT |
| [AngleSharp](https://anglesharp.github.io) | MIT |
| [ClosedXML](https://github.com/ClosedXML/ClosedXML) | MIT |
| [FluentValidation](https://fluentvalidation.net) | Apache-2.0 |
| [Hangfire](https://www.hangfire.io) (Hangfire.AspNetCore, Hangfire.SqlServer) | LGPL-3.0 |
| [HtmlSanitizer](https://github.com/mganss/HtmlSanitizer) | MIT |
| [HtmlToOpenXml](https://github.com/onizet/html2openxml) | MIT |
| [LigerShark.WebOptimizer.Core](https://github.com/ligershark/WebOptimizer) | Apache-2.0 |
| [linq2db](https://linq2db.github.io) | MIT |
| [libphonenumber-csharp](https://github.com/twcclegg/libphonenumber-csharp) | Apache-2.0 |
| [Markdig](https://github.com/xoofx/markdig) | BSD-2-Clause |
| [Microsoft.EntityFrameworkCore.DynamicLinq](https://dynamic-linq.net) | Apache-2.0 |
| [MiniProfiler](https://miniprofiler.com) | MIT |
| [Newtonsoft.Json](https://www.newtonsoft.com/json) | MIT |
| [RestSharp](https://restsharp.dev) | Apache-2.0 |
| [ReverseMarkdown](https://github.com/mysticmind/reversemarkdown-net) | MIT |
| [Riok.Mapperly](https://mapperly.riok.app) | Apache-2.0 |
| [Scrutor](https://github.com/khellang/Scrutor) | MIT |
| [ZiggyCreatures.FusionCache](https://github.com/ZiggyCreatures/FusionCache) | MIT |
| [xUnit](https://xunit.net) (tests only) | Apache-2.0 |

Hangfire is used unmodified as a library, which the LGPL permits in an MIT-licensed application.
