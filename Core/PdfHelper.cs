//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using iTextSharp.tool.xml;
//using iTextSharp.tool.xml.css;
//using iTextSharp.tool.xml.html;
//using iTextSharp.tool.xml.parser;
//using iTextSharp.tool.xml.pipeline.css;
//using iTextSharp.tool.xml.pipeline.end;
//using iTextSharp.tool.xml.pipeline.html;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FichasPTAPI.BLL
//{
//    public class PdfHelper
//    {

//        public MemoryStream ImprimirPdf(string html)
//        {


//            byte[] bytes;
//            using (var ms = new MemoryStream())
//            {

//                //Create an iTextSharp Document which is an abstraction of a PDF but **NOT** a PDF
//                using (var doc = new Document(PageSize.A3))
//                {

//                    //Create a writer that's bound to our PDF abstraction and our stream
//                    using (var writer = PdfWriter.GetInstance(doc, ms))
//                    {

//                        //Open the document for writing
//                        doc.Open();


//                        var tagProcessors = (DefaultTagProcessorFactory)Tags.GetHtmlTagProcessorFactory();
//                        tagProcessors.RemoveProcessor(HTML.Tag.IMG); // remove the default processor
//                        tagProcessors.AddProcessor(HTML.Tag.IMG, new CustomImageTagProcessor()); // use our new processor



//                        using (var msCss = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("")))
//                        {
//                            using (var msHtml = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html)))
//                            {

//                                //Parse the HTML
//                                //iTextSharp.tool.xml.XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, msHtml, msCss);

//                                CssFilesImpl cssFiles = new CssFilesImpl();
//                                cssFiles.Add(XMLWorkerHelper.GetInstance().GetDefaultCSS());
//                                var cssResolver = new StyleAttrCSSResolver(cssFiles);
//                                cssResolver.AddCss("", "utf-8", true);


//                                var charset = Encoding.UTF8;
//                                var hpc = new HtmlPipelineContext(new CssAppliersImpl(new XMLWorkerFontProvider()));
//                                hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(tagProcessors); // inject the tagProcessors

//                                var htmlPipeline = new HtmlPipeline(hpc, new PdfWriterPipeline(doc, writer));
//                                var pipeline = new CssResolverPipeline(cssResolver, htmlPipeline);
//                                var worker = new XMLWorker(pipeline, true);
//                                var xmlParser = new XMLParser(true, worker, charset);
//                                xmlParser.Parse(new StringReader(html));

//                            }
//                        }


//                        doc.Close();
//                    }
//                }

//                //After all of the PDF "stuff" above is done and closed but **before** we
//                //close the MemoryStream, grab all of the active bytes from the stream
//                bytes = ms.ToArray();

//            }

//            var result = new MemoryStream(bytes);
//            return result;

//        }


//    }


//    public class CustomImageTagProcessor : iTextSharp.tool.xml.html.Image
//    {
//        public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent)
//        {
//            IDictionary<string, string> attributes = tag.Attributes;
//            string src;
//            if (!attributes.TryGetValue(HTML.Attribute.SRC, out src))
//                return new List<IElement>(1);

//            if (string.IsNullOrEmpty(src))
//                return new List<IElement>(1);

//            if (src.StartsWith("data:image/", StringComparison.InvariantCultureIgnoreCase))
//            {
//                // data:[<MIME-type>][;charset=<encoding>][;base64],<data>
//                var base64Data = src.Substring(src.IndexOf(",") + 1);
//                var imagedata = Convert.FromBase64String(base64Data);
//                var image = iTextSharp.text.Image.GetInstance(imagedata);

//                var list = new List<IElement>();
//                var htmlPipelineContext = GetHtmlPipelineContext(ctx);
//                list.Add(GetCssAppliers().Apply(new Chunk((iTextSharp.text.Image)GetCssAppliers().Apply(image, tag, htmlPipelineContext), 0, 0, true), tag, htmlPipelineContext));
//                return list;
//            }
//            else
//            {
//                return base.End(ctx, tag, currentContent);
//            }
//        }
//    }
//}
