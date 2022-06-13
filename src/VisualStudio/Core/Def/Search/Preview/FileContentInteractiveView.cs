// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.LanguageServer;
using Microsoft.CodeAnalysis.Navigation;
using Microsoft.CodeAnalysis.Text.Shared.Extensions;
using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Search.Data;
using Microsoft.VisualStudio.Search.UI.PreviewPane.Models;

namespace Microsoft.VisualStudio.LanguageServices.Search.Preview
{
    internal sealed class FileContentInteractiveView : SearchResultInteractiveViewBase
    {
        public FileContentInteractiveView(string title, INavigableItem navigableItem, ImageId icon)
            : base(title, icon)
        {
            this.UserInterface
                = new AbstractCodeEditor(
                    id: "roslynCodeSearchResultCodeEditor",
                    navigableItem.Document.GetURI(),
                    startLine: navigableItem.SourceSpan.Start,
                    startColumn: 0,
                    endLine: navigableItem.SourceSpan.Start,
                    endColumn: 1,
                    isEditable: true);
        }

        public override AbstractUIBase UserInterface { get; }
    }
}
