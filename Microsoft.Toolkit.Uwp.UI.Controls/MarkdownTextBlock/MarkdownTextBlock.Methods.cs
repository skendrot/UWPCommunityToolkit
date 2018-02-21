// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ColorCode;
using Microsoft.Toolkit.Parsers.Markdown;
using Microsoft.Toolkit.Uwp.UI.Controls.Markdown.Render;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// An efficient and extensible control that can parse and render markdown.
    /// </summary>
    public partial class MarkdownTextBlock
    {
        private MarkdownRenderer _renderer;
        private MarkdownDocument _markdown = new MarkdownDocument();

        /// <summary>
        /// Sets the Markdown Renderer for Rendering the UI.
        /// </summary>
        /// <typeparam name="T">The Inherited Markdown Render</typeparam>
        public void SetRenderer<T>()
            where T : MarkdownRenderer
        {
            renderertype = typeof(T);
            _renderer = Activator.CreateInstance(renderertype, _markdown, this, this, this) as MarkdownRenderer;
        }

        /// <summary>
        /// Called to preform a render of the current Markdown.
        /// </summary>
        private void RenderMarkdown()
        {
            // Make sure we have something to parse.
            if (string.IsNullOrWhiteSpace(Text))
            {
                return;
            }

            // Leave if we don't have our root yet.
            if (_rootElement == null)
            {
                return;
            }

            if (_renderer == null)
            {
                return;
            }

            // Disconnect from OnClick handlers.
            UnhookListeners();

            var markdownRenderedArgs = new MarkdownRenderedEventArgs(null);
            try
            {
                // Try to parse the markdown.
                _markdown.Parse(Text);

                // Now try to display it
                _renderer.Background = Background;
                _renderer.BorderBrush = BorderBrush;
                _renderer.BorderThickness = BorderThickness;
                _renderer.CharacterSpacing = CharacterSpacing;
                _renderer.FontFamily = FontFamily;
                _renderer.FontSize = FontSize;
                _renderer.FontStretch = FontStretch;
                _renderer.FontStyle = FontStyle;
                _renderer.FontWeight = FontWeight;
                _renderer.Foreground = Foreground;
                _renderer.IsTextSelectionEnabled = IsTextSelectionEnabled;
                _renderer.Padding = Padding;
                _renderer.CodeBackground = CodeBackground;
                _renderer.CodeBorderBrush = CodeBorderBrush;
                _renderer.CodeBorderThickness = CodeBorderThickness;
                _renderer.InlineCodeBorderThickness = InlineCodeBorderThickness;
                _renderer.InlineCodeBackground = InlineCodeBackground;
                _renderer.InlineCodeBorderBrush = InlineCodeBorderBrush;
                _renderer.InlineCodePadding = InlineCodePadding;
                _renderer.InlineCodeFontFamily = InlineCodeFontFamily;
                _renderer.CodeForeground = CodeForeground;
                _renderer.CodeFontFamily = CodeFontFamily;
                _renderer.CodePadding = CodePadding;
                _renderer.CodeMargin = CodeMargin;
                _renderer.EmojiFontFamily = EmojiFontFamily;
                _renderer.Header1FontSize = Header1FontSize;
                _renderer.Header1FontWeight = Header1FontWeight;
                _renderer.Header1Margin = Header1Margin;
                _renderer.Header1Foreground = Header1Foreground;
                _renderer.Header2FontSize = Header2FontSize;
                _renderer.Header2FontWeight = Header2FontWeight;
                _renderer.Header2Margin = Header2Margin;
                _renderer.Header2Foreground = Header2Foreground;
                _renderer.Header3FontSize = Header3FontSize;
                _renderer.Header3FontWeight = Header3FontWeight;
                _renderer.Header3Margin = Header3Margin;
                _renderer.Header3Foreground = Header3Foreground;
                _renderer.Header4FontSize = Header4FontSize;
                _renderer.Header4FontWeight = Header4FontWeight;
                _renderer.Header4Margin = Header4Margin;
                _renderer.Header4Foreground = Header4Foreground;
                _renderer.Header5FontSize = Header5FontSize;
                _renderer.Header5FontWeight = Header5FontWeight;
                _renderer.Header5Margin = Header5Margin;
                _renderer.Header5Foreground = Header5Foreground;
                _renderer.Header6FontSize = Header6FontSize;
                _renderer.Header6FontWeight = Header6FontWeight;
                _renderer.Header6Margin = Header6Margin;
                _renderer.Header6Foreground = Header6Foreground;
                _renderer.HorizontalRuleBrush = HorizontalRuleBrush;
                _renderer.HorizontalRuleMargin = HorizontalRuleMargin;
                _renderer.HorizontalRuleThickness = HorizontalRuleThickness;
                _renderer.ListMargin = ListMargin;
                _renderer.ListGutterWidth = ListGutterWidth;
                _renderer.ListBulletSpacing = ListBulletSpacing;
                _renderer.ParagraphMargin = ParagraphMargin;
                _renderer.QuoteBackground = QuoteBackground;
                _renderer.QuoteBorderBrush = QuoteBorderBrush;
                _renderer.QuoteBorderThickness = QuoteBorderThickness;
                _renderer.QuoteForeground = QuoteForeground;
                _renderer.QuoteMargin = QuoteMargin;
                _renderer.QuotePadding = QuotePadding;
                _renderer.TableBorderBrush = TableBorderBrush;
                _renderer.TableBorderThickness = TableBorderThickness;
                _renderer.TableCellPadding = TableCellPadding;
                _renderer.TableMargin = TableMargin;
                _renderer.TextWrapping = TextWrapping;
                _renderer.LinkForeground = LinkForeground;
                _renderer.ImageStretch = ImageStretch;
                _renderer.WrapCodeBlock = WrapCodeBlock;

                _rootElement.Child = _renderer.Render();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error while parsing and rendering: " + ex.Message);
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                markdownRenderedArgs = new MarkdownRenderedEventArgs(ex);
            }

            // Indicate that the parse is done.
            MarkdownRendered?.Invoke(this, markdownRenderedArgs);
        }

        private void UnhookListeners()
        {
            // Clear any hyper link events if we have any
            foreach (Hyperlink link in _listeningHyperlinks)
            {
                link.Click -= Hyperlink_Click;
            }

            // Clear everything that exists.
            _listeningHyperlinks.Clear();
        }

        /// <summary>
        /// Called when the render has a link we need to listen to.
        /// </summary>
        public void RegisterNewHyperLink(Hyperlink newHyperlink, string linkUrl)
        {
            // Setup a listener for clicks.
            newHyperlink.Click += Hyperlink_Click;

            // Associate the URL with the hyperlink.
            newHyperlink.SetValue(HyperlinkUrlProperty, linkUrl);

            // Add it to our list
            _listeningHyperlinks.Add(newHyperlink);
        }

        /// <summary>
        /// Called when the renderer needs to display a image.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        async Task<ImageSource> IImageResolver.ResolveImageAsync(string url, string tooltip)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                if (!string.IsNullOrEmpty(UriPrefix))
                {
                    url = string.Format("{0}{1}", UriPrefix, url);
                }
            }

            var eventArgs = new ImageResolvingEventArgs(url, tooltip);
            ImageResolving?.Invoke(this, eventArgs);

            await eventArgs.WaitForDeferrals();

            try
            {
                return eventArgs.Handled
                    ? eventArgs.Image
                    : GetImageSource(new Uri(url));
            }
            catch (Exception)
            {
                return null;
            }

            ImageSource GetImageSource(Uri imageUrl)
            {
                if (_isSvgImageSupported)
                {
                    if (Path.GetExtension(imageUrl.AbsolutePath)?.ToLowerInvariant() == ".svg")
                    {
                        return new SvgImageSource(imageUrl);
                    }
                }

                return new BitmapImage(imageUrl);
            }
        }

        /// <summary>
        /// Called when a Code Block is being rendered.
        /// </summary>
        /// <returns>Parsing was handled Successfully</returns>
        bool ICodeBlockResolver.ParseSyntax(InlineCollection inlineCollection, string text, string codeLanguage)
        {
            var eventArgs = new CodeBlockResolvingEventArgs(inlineCollection, text, codeLanguage);
            CodeBlockResolving?.Invoke(this, eventArgs);

            try
            {
                var result = eventArgs.Handled;
                if (UseSyntaxHighlighting && !result && codeLanguage != null)
                {
                    var language = Languages.FindById(codeLanguage);
                    if (language != null)
                    {
                        RichTextBlockFormatter formatter;
                        if (CodeStyling != null)
                        {
                            formatter = new RichTextBlockFormatter(CodeStyling);
                        }
                        else
                        {
                            var theme = themeListener.CurrentTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
                            if (RequestedTheme != ElementTheme.Default)
                            {
                                theme = RequestedTheme;
                            }

                            formatter = new RichTextBlockFormatter(theme);
                        }

                        formatter.FormatInlines(text, language, inlineCollection);
                        return true;
                    }
                }

                return result;
            }
            catch
            {
                return false;
            }
        }
    }
}