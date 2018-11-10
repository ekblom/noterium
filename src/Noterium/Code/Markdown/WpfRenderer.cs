// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license.
// See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using Markdig.Annotations;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Wpf;
using Markdig.Renderers.Wpf.Extensions;
using Markdig.Renderers.Wpf.Inlines;
using Markdig.Syntax;
using Markdig.Wpf;
using Noterium.Properties;
using Block = System.Windows.Documents.Block;

namespace Noterium.Code.Markdown
{
    /// <summary>
    /// WPF renderer for a Markdown <see cref="MarkdownDocument"/> object.
    /// </summary>
    /// <seealso cref="RendererBase" />
    public class WpfRenderer : Markdig.Renderers.WpfRenderer
    {

        public WpfRenderer([NotNull] FlowDocument document) : base(document)
        {
            ObjectRenderers.RemoveAt(ObjectRenderers.FindIndex(x => x is Markdig.Renderers.Wpf.Extensions.TaskListRenderer));
            ObjectRenderers.Add(new TaskListRenderer());
        }
    }
}
