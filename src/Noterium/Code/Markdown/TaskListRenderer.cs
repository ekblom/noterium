// Copyright (c) 2016-2017 Nicolas Musset. All rights reserved.
// This file is licensed under the MIT license. 
// See the LICENSE.md file in the project root for more information.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Markdig.Extensions.TaskLists;
using Markdig.Renderers.Wpf;
using Markdig.Wpf;
using Noterium.Properties;

namespace Noterium.Code.Markdown
{
    public class TaskListRenderer : Markdig.Renderers.Wpf.Extensions.TaskListRenderer
    {
        public static RoutedCommand TaskListItemChanged { get; } = new RoutedCommand(nameof(Hyperlink), typeof(TaskListRenderer));

        private int _checkCount = 0;
        protected override void Write([NotNull] Markdig.Renderers.WpfRenderer renderer, [NotNull] TaskList taskList)
        {
            var checkBox = new CheckBox
            {
                IsChecked = taskList.Checked,
                Tag = _checkCount,
                Command = TaskListItemChanged
            };

            checkBox.CommandParameter = checkBox;

            _checkCount++;

            checkBox.SetResourceReference(FrameworkContentElement.StyleProperty, Styles.TaskListStyleKey);
            renderer.WriteInline(new InlineUIContainer(checkBox));
        }
    }
}
